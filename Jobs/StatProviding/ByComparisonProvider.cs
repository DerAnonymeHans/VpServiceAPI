using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Entities.Statistics;
using VpServiceAPI.Enums;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.StatProviding
{
    public class ByComparisonProvider : IByComparisonProvider
    {
        private readonly IDataQueries DataQueries;
        private readonly IMyLogger Logger;
        public ByComparisonProvider(IDataQueries dataQueries, IMyLogger logger)
        {
            DataQueries = dataQueries;
            Logger = logger;
        }
        private async Task<EntityType> GetEntityType(string name)
        {
            var typeRes = await DataQueries.Load<string, dynamic>("SELECT type FROM stat_entities WHERE BINARY name = @name AND year = @year", new { name, year=ProviderHelper.GetYear() });
            if (typeRes.Count == 0) throw new NameNotFoundException(name);
            Enum.TryParse<EntityType>(typeRes[0], out var type);
            return type;
        }
        private async Task<int> GetTotalLessons(string name, EntityType type)
        {
            double totalLessons;
            // <WOCHENSTUNDEN> / 5 * <ANZAHL AUFGEZEICHNETER TAGE>
            // if name and type are found in hard_stat_data
            string selectSql = "SELECT (d.value / 5 * (SELECT COUNT(DISTINCT(vp_data.date)) FROM vp_data WHERE year = @year)) AS total_lessons FROM hard_stat_data d";
            var res = await DataQueries.Load<double, dynamic>($"{selectSql} WHERE d.category='WEEKLY_LESSONS' AND type=@type AND BINARY d.name=@name AND year=@year", new { name, type = type.ToString(), year = ProviderHelper.GetYear() });

            if (res.Count == 0)
            {
                string whereSql;
                if(type == EntityType.CLASS)
                {
                    // retrieve grade from class
                    name = Regex.Match(name, @"\d+").Value;
                    whereSql = $"type='CLASS' AND name=@name";
                }else if(type == EntityType.KURS)
                {
                    name = Regex.Match(name, @"(?<=/\s)[a-zA-Z]+").Value;
                    whereSql = $"type='KURS' AND BINARY name=@name";
                }
                else
                {
                    whereSql = $"type=@type AND name='average'";
                }
                
                res = await DataQueries.Load<double, dynamic>($"{selectSql} WHERE category='WEEKLY_LESSONS' AND {whereSql}", new { name, type=type.ToString(), year = ProviderHelper.GetYear() });
                if (res.Count == 0) throw new NameNotFoundException(name);
                totalLessons = res[0];
            }
            else totalLessons = res[0];
            return (int)totalLessons;
        }
        public async Task<RelativeStatistic> RelativeOf(string name)
        {
            // <FEHLSTUNDEN> / <GESAMTSTUNDEN>
            int totalLessons = await GetTotalLessons(name, await GetEntityType(name));
            var res = await DataQueries.Load<RelativeStatistic, dynamic>("SELECT e.name, e.type, ((c.missed - c.substituted) / @totalLessons) AS missed, (c.substituted / @totalLessons) AS substituted, @totalLessons AS hundredPercent FROM stat_entities e INNER JOIN stats_by_count c ON c.entity_id = e.id WHERE BINARY e.name=@name AND e.year=@year", new { totalLessons, name, year = ProviderHelper.GetYear() });
            if (res.Count == 0) throw new NameNotFoundException(name);

            return new RelativeStatistic(res[0].Name, res[0].Missed, res[0].Substituted, totalLessons) { Type = res[0].Type };
        }
        public async Task<List<RelativeStatistic>> SortRelativeOf(EntityType includeWho, string sortBy)
        {
            //SELECT
            //e.name, 
            //e.type, 
            //((c.missed - c.substituted) / ((SELECT `value` FROM hard_stat_data WHERE category = 'WEEKLY_LESSONS' AND type = 'KURS' AND BINARY name LIKE CONCAT('%', SUBSTR(e.name, 7, 2), '%') LIMIT 1) / 5 * @days)) AS missed
            //(c.substituted / ((SELECT `value` FROM hard_stat_data WHERE category = 'WEEKLY_LESSONS' AND type = 'KURS' AND BINARY name LIKE CONCAT('%', SUBSTR(e.name, 7, 2), '%') LIMIT 1) / 5 * @days)) AS substituted
            //FROM stat_entities e
            //INNER JOIN stats_by_count c ON c.entity_id = e.id
            //WHERE e.type = 'KURS'
            int days = (await DataQueries.Load<int, dynamic>("SELECT COUNT(DISTINCT(vp_data.date)) FROM vp_data WHERE year=@year", new { year = ProviderHelper.GetYear() }))[0];

            string sortSql = sortBy switch
            {
                "mm" => "missed DESC",
                "lm" => "missed",
                "ms" => "substituted DESC",
                "ls" => "substituted",
                _ => throw new SortNotFoundException(sortBy)
            };

            string totalLessonsSelectSql = "SELECT `value` FROM hard_stat_data WHERE category = 'WEEKLY_LESSONS' AND year=@year";

            string totalLessonsWhereSql = includeWho switch
            {
                EntityType.KURS => "type='KURS' AND BINARY name LIKE CONCAT('%', SUBSTR(e.name, 7, 2), '%')",
                EntityType.CLASS => @"type='CLASS' AND BINARY name LIKE CONCAT('%', REGEXP_SUBSTR(e.name, '\d+'), '%')",
                _ => "type=@type AND BINARY name=e.name"
            };

            string totalLessonsSql = $"(COALESCE(({totalLessonsSelectSql} AND {totalLessonsWhereSql} LIMIT 1), ({totalLessonsSelectSql} AND type=@type AND name='average')) / 5 * @days)";

            string sql =
                $@"
                    SELECT e.name, e.type, {totalLessonsSql} AS hundredPercent, c.missed - c.substituted AS missedAbs, c.substituted AS substitutedAbs,
                    ((c.missed - c.substituted) / {totalLessonsSql}) AS missed,
                    (c.substituted / {totalLessonsSql}) AS substituted
                    FROM stat_entities e
                    INNER JOIN stats_by_count c ON c.entity_id = e.id
                    WHERE e.type = @type AND e.year=@year
                    ORDER BY {sortSql} LIMIT 10
                ";

            var res = await DataQueries.Load<RelativeStatistic, dynamic>(sql, new { type=includeWho.ToString(), days, year = ProviderHelper.GetYear() });

            return res;
        }
        public async Task<ComparisonStatistic> RelativeOfInComparison(string name)
        {
            // <RELATIVE FEHLSTUNDEN> / <DURCHSCHNITTLICHE RELATIVE FEHLSTUNDEN DES TYPS>
            var specific = await RelativeOf(name);
            Enum.TryParse<EntityType>(specific.Type, out var type);
            RelativeStatistic average;
            if(type == EntityType.KEPLER)
            {
                average = new RelativeStatistic
                {
                    Missed = 0.056M,
                    Substituted = 0.047M
                };
            }
            else
            {
                average = await RelativeOfType(type);
            }

            return new ComparisonStatistic
            {
                Name = specific.Name,
                Type = specific.Type.ToString(),
                Missed = (specific.Missed / average.Missed) - 1,
                Substituted = (specific.Substituted / average.Substituted) - 1,
                MissedAverage = average.Missed,
                SubstitutedAverage = average.Substituted
            };
                
        }
        public async Task<RelativeStatistic> RelativeOfType(EntityType type)
        {
            int totalLessons = await GetTotalLessons("average", type);

            int typeCount = (await DataQueries.Load<int, dynamic>("SELECT COUNT(id) FROM stat_entities WHERE type=@type AND year=@year", new { type = type.ToString(), year = ProviderHelper.GetYear() }))[0];

            string selectSql = "SELECT SUM(((c.missed - c.substituted)) / (@totalLessons * @typeCount)) AS missed, SUM(c.substituted) / (@totalLessons * @typeCount) AS substituted FROM stat_entities e INNER JOIN stats_by_count c ON c.entity_id = e.id WHERE e.type=@type AND e.year=@year";

            var res = await DataQueries.Load<RelativeStatistic, dynamic>($"{selectSql};", new { totalLessons, type=type.ToString(), typeCount, year = ProviderHelper.GetYear() });
            return res[0];
        }
        public async Task<CountStatistic> AverageOf(EntityType type)
        {
            // <GESAMTFEHLSTUNDEN DES TYPS> / <ANZAHL ENTITÄTEN DES TYPS>
            CountStatistic total = (await DataQueries.Load<CountStatistic, dynamic>("SELECT SUM(missed) AS missed, SUM(substituted) AS substituted FROM stat_entities e INNER JOIN stats_by_count c ON e.id = c.entity_id WHERE e.type=@type AND e.year=@year", new { type = type.ToString(), year = ProviderHelper.GetYear() }))[0];
            int entityCount = (await DataQueries.Load<int, dynamic>("SELECT `value` FROM hard_stat_data WHERE category='COUNT' AND type=@type AND year=@year", new { type = type.ToString(), year = ProviderHelper.GetYear() }))[0];
            //int entityCount = (await DataQueries.Load<int, dynamic>("SELECT COUNT(id) AS `count` FROM `stat_entities` WHERE type='TEACHER' AND year=@year", new { year = ProviderHelper.GetYear() }))[0];
            return new CountStatistic("", type.ToString(), (total.Missed - total.Substituted) / entityCount, total.Substituted / entityCount);
        }

        
    }

    
}
