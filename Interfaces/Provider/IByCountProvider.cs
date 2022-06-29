using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Statistics;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Interfaces
{
    public interface IByCountProvider
    {
        public Task<CountStatistic> GetCountOf(string name);
        public Task<List<CountStatistic>> GetSortedCountsBy(EntityType includeWho, string sortBy);
    }
}
