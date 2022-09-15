using System.Threading.Tasks;
using VpServiceAPI.Entities.Plan;

namespace VpServiceAPI.Interfaces
{
    public interface INotificationJob
    {
        public void Begin(PlanCollection plans);
        public Task DeleteCache();
    }
}
