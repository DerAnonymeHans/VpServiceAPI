using System.Threading.Tasks;
using VpServiceAPI.Entities.Plan;

namespace VpServiceAPI.Interfaces
{
    public interface IPlanConverter
    {
        public PlanModel? Convert(string planHTML);
    }
}
