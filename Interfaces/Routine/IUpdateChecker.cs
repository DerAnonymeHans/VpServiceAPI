using System.Threading.Tasks;
using VpServiceAPI.Entities.Plan;
using VpServiceAPI.Entities.Tools;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Interfaces
{
    public interface IUpdateChecker
    {
        public Task<StatusWrapper<UpdateCheckStatus, PlanModel>> Check(WhatPlan whatPlan, int dayShift=0);
    }
}
