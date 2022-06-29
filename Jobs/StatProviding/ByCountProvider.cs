﻿using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Statistics;
using VpServiceAPI.Enums;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.StatProviding
{
    public class ByCountProvider : IByCountProvider
    {
        private readonly IDataQueries DataQueries;
        public ByCountProvider(IDataQueries dataQueries)
        {
            DataQueries = dataQueries;
        }
        public async Task<CountStatistic> GetCountOf(string name)
        {
            var res = await DataQueries.Load<CountStatistic, dynamic>("SELECT name, type, missed - substituted as missed, substituted FROM stat_entities e INNER JOIN stats_by_count c ON e.id = c.entity_id WHERE BINARY e.name=@name", new { name = name });
            if (res.Count == 0) throw new NameNotFoundException(name);
            return res[0];
        }

        public async Task<List<CountStatistic>> GetSortedCountsBy(EntityType includeWho, string sortBy)
        {
            string sortSql = sortBy switch
            {
                "mm" => "missed DESC",
                "lm" => "missed",
                "ms" => "substituted DESC",
                "ls" => "substituted",
                _ => throw new SortNotFoundException(sortBy)
            };
            var res = await DataQueries.Load<CountStatistic, dynamic>($"SELECT name, type, missed - substituted as missed, substituted FROM stat_entities e INNER JOIN stats_by_count c ON e.id = c.entity_id WHERE e.type=@type ORDER BY {sortSql} LIMIT @limit", new { type = includeWho.ToString(), limit = 10});
            return res;
        }
    }
}
