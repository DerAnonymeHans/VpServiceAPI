using System.Threading.Tasks;
using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface IPlanConverter
    {
        public PlanModel? Convert(string planHTML);
    }
}
