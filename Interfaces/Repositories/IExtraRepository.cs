using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Notification;

namespace VpServiceAPI.Interfaces
{
    public interface IExtraRepository
    {
        public Task<SmallExtra> GetRandSmallExtra();
        public Task AddSmallExtraProposal(SmallExtra smallExtra);
        public Task AcceptProposal(string text);
        public Task RejectProposal(string text);
        public Task<List<SmallExtra>> GetProposals();
    }
}
