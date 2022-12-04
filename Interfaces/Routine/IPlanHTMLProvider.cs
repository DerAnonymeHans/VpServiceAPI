using System;
using System.Net.Http;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Entities.Tools;
using VpServiceAPI.Enums;

#nullable enable
namespace VpServiceAPI.Interfaces
{
    public interface IPlanHTMLProvider
    {
        public Task<StatusWrapper<PlanProvideStatus, string>> GetPlanHTML(DateTime date);
    }
}
