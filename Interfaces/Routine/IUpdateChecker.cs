using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Entities.Plan;

namespace VpServiceAPI.Interfaces
{
    public interface IUpdateChecker
    {
        public Task<StatusWrapper<PlanModel>> Check(WhatPlan whatPlan, int dayShift=0);
    }
}
