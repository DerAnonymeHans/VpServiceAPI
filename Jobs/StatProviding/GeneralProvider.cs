using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Enums;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.StatProviding
{
    public class GeneralProvider : IByGeneralProvider
    {
        private readonly IDataQueries DataQueries;
        private readonly IMyLogger Logger;
        public GeneralProvider(IDataQueries dataQueries, IMyLogger logger)
        {
            DataQueries = dataQueries;
            Logger = logger;
        }

        public async Task<int> GetDaysCount()
        {
            return (await DataQueries.Load<int, dynamic>("SELECT COUNT(DISTINCT(date)) FROM vp_data WHERE year=@year", new { year = ProviderHelper.GetYear() }))[0];
        }

        public async Task<List<string>> GetNames(EntityType entityType)
        {
            return await DataQueries.Load<string, dynamic>("SELECT name FROM stat_entities WHERE type=@type AND year=@year", new { type = entityType.ToString(), year = ProviderHelper.GetYear() });
        }

        public async Task<List<string>> GetYears()
        {
            return await DataQueries.Load<string, dynamic>("SELECT DISTINCT(year) FROM `hard_stat_data` WHERE 1", new { });
        }
    }
}
