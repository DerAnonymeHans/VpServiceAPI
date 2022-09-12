using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Statistics;
using VpServiceAPI.Enums;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.StatProviding
{
    public sealed class ByWhoProvider : IByWhoProvider
    {
        private readonly IDataQueries DataQueries;
        private readonly IMyLogger Logger;
        public ByWhoProvider(IDataQueries dataQueries, IMyLogger logger)
        {
            DataQueries = dataQueries;
            Logger = logger;
        }
        public async Task<WhoStatistic> GetRelationToSpecific(string name, string otherName)
        {
            var res = await DataQueries.Load<WhoStatistic, dynamic>("SELECT e1.name, e1.type, e2.name AS other_name, e2.type AS other_type, w.missed AS missed, w.substituted FROM stats_by_who w INNER JOIN stat_entities e1 ON w.entity_id_a = e1.id INNER JOIN stat_entities e2 ON w.entity_id_b = e2.id WHERE (BINARY e1.name = @name OR BINARY e2.name = @name) AND (BINARY e1.name = @otherName OR BINARY e2.name = @otherName) AND (e1.year=@year AND e2.year=@year)", new { name, otherName, year = ProviderHelper.GetYear() });
            if (res.Count == 0) throw new AppException($"Es besteht keine Verknüpfung zwischen '{name}' und '{otherName}'.");
            res[0].Missed -= res[0].Substituted;
            return res[0].Name == name ? res[0] : new WhoStatistic(res[0].OtherName, res[0].OtherType?.ToString() ?? "", res[0].Name, res[0].Type?.ToString() ?? "", res[0].Missed, res[0].Substituted);
        }

        public async Task<List<WhoStatistic>> GetRelationToType(string name, EntityType entityType)
        {
            var res = await DataQueries.Load<WhoStatistic, dynamic>("SELECT e1.name, e1.type, e2.name AS other_name, e2.type AS other_type, w.missed, w.substituted FROM stats_by_who w INNER JOIN stat_entities e1 ON w.entity_id_a = e1.id INNER JOIN stat_entities e2 ON w.entity_id_b = e2.id WHERE (BINARY e1.name = @name OR BINARY e2.name = @name) AND (e1.type = @otherType OR e2.type = @otherType) AND e1.year=@year AND e2.year=@year", new { name, otherType = entityType.ToString(), year = ProviderHelper.GetYear() });
            if (res.Count == 0) throw new NameNotFoundException(name);
            var newList = new List<WhoStatistic>();
            foreach(var stat in res)
            {
                stat.Missed -= stat.Substituted;
                newList.Add(stat.Name == name ? stat : new WhoStatistic(stat.OtherName, stat.OtherType?.ToString() ?? "", stat.Name, stat.Type?.ToString() ?? "", stat.Missed, stat.Substituted));
            }
            return newList;
        }

        public async Task<List<WhoStatistic>> SortRelations(string sortBy)
        {
            sortBy = sortBy switch
            {
                "mm" => "w.missed DESC",
                "lm" => "w.missed",
                "ms" => "w.substituted DESC",
                "ls" => "w.substituted",
                _ => throw new SortNotFoundException(sortBy)
            };
            var res = await DataQueries.Load<WhoStatistic, dynamic>($"SELECT e1.name, e1.type, e2.name AS other_name, e2.type AS other_type, w.missed, w.substituted FROM stats_by_who w INNER JOIN stat_entities e1 ON w.entity_id_a = e1.id INNER JOIN stat_entities e2 ON w.entity_id_b = e2.id WHERE e1.year=@year AND e2.year=@year ORDER BY {sortBy} LIMIT 20", new { year = ProviderHelper.GetYear() });
            return res;
        }
    }
}

//SELECT
//(SELECT name FROM stat_entities e WHERE e.id = w.entity_id_a) AS party_a,
// (SELECT name FROM stat_entities e WHERE e.id = w.entity_id_b) AS party_b,
//w.missed,
//w.substituted
// FROM stats_by_who w
// WHERE w.entity_id_a=ID OR w.entity_id_b=ID

//SELECT e1.name, e1.type, e2.name AS other_name, e2.type AS other_type, w.missed, w.substituted
//FROM stats_by_who w

//INNER JOIN stat_entities e1 ON w.entity_id_a= e1.id

//INNER JOIN stat_entities e2 ON w.entity_id_b= e2.id

//WHERE e1.name= 'MA' OR e2.name= 'MA'
