using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Statistics;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.StatProviding
{
    public class ByMetaProvider : IByMetaProvider
    {
        private readonly IDataQueries DataQueries;
        private readonly IMyLogger Logger;
        public ByMetaProvider(IDataQueries dataQueries, IMyLogger logger)
        {
            DataQueries = dataQueries;
            Logger = logger;
        }
        public async Task<List<KeyCountStatistic>> SortCountsOfExtras(string sortBy)
        {
            List<KeyCountStatistic> specialCases;
            KeyCountStatistic standardCase;
            switch (sortBy)
            {
                case "mm":
                    specialCases = await DataQueries.Load<KeyCountStatistic, dynamic>("SELECT extra AS `key`, COUNT(*) AS `count`, 'missed' AS type FROM vp_data WHERE type='AUS' AND extra NOT LIKE '%fällt aus%' GROUP BY extra ORDER BY COUNT(*) DESC", new { });
                    standardCase = (await DataQueries.Load<KeyCountStatistic, dynamic>("SELECT 'fällt aus' AS `key`, COUNT(*) AS `count`, 'missed' AS type FROM vp_data WHERE type='AUS' AND extra LIKE '%fällt aus%'", new { }))[0];
                    break;
                case "ms":
                    specialCases = await DataQueries.Load<KeyCountStatistic, dynamic>("SELECT extra AS `key`, COUNT(*) AS `count`, 'substituted' AS type FROM vp_data WHERE type='VER' AND extra NOT LIKE '%für%' GROUP BY extra ORDER BY COUNT(*) DESC", new { });
                    standardCase = (await DataQueries.Load<KeyCountStatistic, dynamic>("SELECT 'für' AS `key`, COUNT(*) AS `count`, 'substituted' AS type FROM vp_data WHERE type='VER' AND extra LIKE '%für%'", new { }))[0];
                    break;
                default:
                    throw new SortNotFoundException(sortBy);

            }
            int specialCasesCount = 9;
            var extras = new List<KeyCountStatistic>();
            extras.Add(standardCase);
            extras.AddRange(specialCases.GetRange(0, specialCases.Count < specialCasesCount ? specialCases.Count : specialCasesCount));
            //extras.Add(new KeyCountStatistic
            //{
            //    Key = "sonstiges",
            //    Type = sortBy == "ms" ? "missed" : "substituted"
            //});
            //try
            //{
            //    extras.Last().Count = specialCases.GetRange(specialCasesCount, specialCases.Count - specialCasesCount).Sum(stat => stat.Count);
            //}
            //catch
            //{
            //    extras.Last().Count = 0;
            //}

            return extras.OrderByDescending(el => el.Count).ToList();
        }
    }
}
