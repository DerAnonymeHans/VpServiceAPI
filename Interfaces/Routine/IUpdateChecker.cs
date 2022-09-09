using System.Threading.Tasks;
using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface IUpdateChecker
    {
        public Task<StatusWrapper<PlanModel>> Check(bool secondPlan=false, int dayShift=0);
    }
}
