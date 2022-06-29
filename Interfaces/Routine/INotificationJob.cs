using System.Threading.Tasks;
using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface INotificationJob
    {
        public void Begin(PlanModel planModel);
        public Task DeleteCache();
    }
}
