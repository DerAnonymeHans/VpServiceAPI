using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Statistics;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Interfaces
{
    public interface IByComparisonProvider
    {
        public Task<RelativeStatistic> RelativeOf(string name);
        public Task<RelativeStatistic> RelativeOfType(EntityType type);
        public Task<List<RelativeStatistic>> SortRelativeOf(EntityType includWho, string sortBy);
        public Task<ComparisonStatistic> RelativeOfInComparison(string name);
        public Task<CountStatistic> AverageOf(EntityType type);
    }
}
