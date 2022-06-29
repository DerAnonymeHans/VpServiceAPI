using System.Net.Http;
using System.Threading.Tasks;
using VpServiceAPI.Entities;

#nullable enable
namespace VpServiceAPI.Interfaces
{
    public interface IPlanHTMLProvider
    {
        public Task<string> GetPlanHTML(int daysFromToday);
    }
}
