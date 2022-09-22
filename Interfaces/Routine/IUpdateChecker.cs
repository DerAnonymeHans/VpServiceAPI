using System.Threading.Tasks;
using VpServiceAPI.Entities.Plan;
using VpServiceAPI.Entities.Tools;

namespace VpServiceAPI.Interfaces
{
    public interface IUpdateChecker
    {
        public Task<StatusWrapper<PlanModel>> Check(WhatPlan whatPlan, int dayShift=0);
    }
}
