using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Statistics;

namespace VpServiceAPI.Interfaces
{
    public interface IByMetaProvider
    {
        public Task<List<KeyCountStatistic>> SortCountsOfExtras(string sortBy);
    }
}
