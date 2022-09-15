using System.Collections.Generic;

namespace VpServiceAPI.Entities.Plan
{
    public sealed class PlanCollection
    {
        public List<PlanModel> Plans { get; set; } = new();
        public PlanModel FirstPlan => Plans[0];
        public ForceNotifStatus ForceNotifStatus { get; } = new();
        public void Add(PlanModel plan)
        {
            Plans.Add(plan);
            ForceNotifStatus.TrySet(plan.ForceNotifStatus);
        }
    }
}
