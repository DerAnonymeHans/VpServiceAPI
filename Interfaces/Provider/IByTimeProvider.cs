using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Statistics;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Interfaces
{
    public interface IByTimeProvider
    {
        public Task<TimeStatistic> GetTimesOf(TimeType timeType, string name);
        public Task<List<CountStatistic>> GetSortedTimeBy(TimeType timeType, string timeName, EntityType includeWho, string sortBy);
    }
}
