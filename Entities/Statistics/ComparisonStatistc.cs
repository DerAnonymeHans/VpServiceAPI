
using System;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Entities.Statistics
{
    public class ComparisonStatistic : BaseStatistic
    {
        public decimal Missed { get; set; }
        public decimal Substituted { get; set; }
        public decimal MissedAverage { get; set; }
        public decimal SubstitutedAverage { get; set; }

        public ComparisonStatistic() : base("")
        {

        }
    }
}