using System;

namespace VpServiceAPI.Entities.Statistics
{
    public class RelativeStatistic : BaseStatistic
    {
        public decimal Missed { get; set; }
        public decimal Substituted { get; set; }
        public decimal HundredPercent { get; set; }

        public int MissedAbs { get; set; }
        public int SubstitutedAbs { get; set; }

        public RelativeStatistic() : base("")
        {

        }
        public RelativeStatistic(string name, decimal missed, decimal substituted, decimal hundredPercent) : base(name)
        {
            Missed = missed;
            Substituted = substituted;
            HundredPercent = hundredPercent;
        }
    }
}
