
using System;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Entities.Statistics
{
    public class WhoStatistic : BaseStatistic
    {
        public string OtherName { get; set; }
        public string? OtherType { get; set; }

        public int Missed { get; set; }
        public int Substituted { get; set; }

        public WhoStatistic(string name, string type, string other_name, string other_type, int missed, int substituted) : base(name)
        {
            Type = type;
            OtherName = other_name;
            OtherType = other_type;
            Missed = missed;
            Substituted = substituted;
        }
    }
}