using System.Collections.Generic;

namespace VpServiceAPI.Entities.Plan
{
    public sealed class GlobalPlan
    {
        public string AffectedDate { get; set; } = "";
        public string OriginDate { get; set; } = "";
        public string OriginTime { get; set; } = "";
        public string AffectedWeekday { get; set; } = "";
        public List<string> Announcements { get; set; } = new();
        public List<string> MissingTeachers { get; set; } = new();
        
    }
}
