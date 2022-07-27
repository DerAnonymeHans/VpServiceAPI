using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Interfaces
{
    public interface IByGeneralProvider
    {
        public Task<List<string>> GetNames(EntityType entityType);
        public Task<int> GetDaysCount();
        public Task<List<string>> GetYears();
        public Task CheckDataFreshness();
    }
}
