using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Statistics;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Interfaces
{
    public interface IByWhoProvider
    {
        public Task<WhoStatistic> GetRelationToSpecific(string name, string otherName);
        public Task<List<WhoStatistic>> GetRelationToType(string name, EntityType entityType);
        public Task<List<WhoStatistic>> SortRelations(string sortBy);
    }
}
