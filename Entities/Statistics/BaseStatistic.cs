using System;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Entities.Statistics
{
    public class BaseStatistic
    {
        public string Name { get; set; }
        public string? Type { get; set; }

        public BaseStatistic(string name)
        {
            Name = name;
        }
    }
}
