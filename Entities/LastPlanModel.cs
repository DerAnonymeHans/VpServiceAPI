using System.Collections.Generic;
using VpServiceAPI.Entities.Plan;

namespace VpServiceAPI.Entities
{
    public class LastPlanModel
    {
        public string AffectedWeekday { get; init; }
        public string AffectedDate { get; init; }
        public List<PlanRow> Rows { get; init; }
        public List<string> Information { get; init; }
    }
}
