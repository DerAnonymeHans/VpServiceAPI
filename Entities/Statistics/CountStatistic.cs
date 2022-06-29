
using System;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Entities.Statistics
{
    public class CountStatistic : BaseStatistic
    {
        public decimal Missed { get; set; }
        public decimal Substituted { get; set; }

        public CountStatistic() : base("")
        {

        }

        public CountStatistic(string name, string type, decimal missed, decimal substituted) : base(name)
        {
            Name = name;
            Type = type;
            Missed = missed;
            Substituted = substituted;
        }
        public CountStatistic(string name, string type, long missed, int substituted) : base(name)
        {
            Name = name;
            Type = type;
            Missed = missed;
            Substituted = substituted;
        }
        public CountStatistic(int missed, int substituted) : base("")
        {
            Missed = missed;
            Substituted = substituted;
        }
        public CountStatistic(decimal missed, decimal substituted) : base("")
        {
            Missed = missed;
            Substituted = substituted;
        }
    }
}