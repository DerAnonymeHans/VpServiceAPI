using System.Threading.Tasks;
using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface IUpdateChecker
    {
        public Task<bool?> Check(bool secondPlan=false, int dayShift=0);
        public PlanModel PlanModel { get; set; }
    }
}
