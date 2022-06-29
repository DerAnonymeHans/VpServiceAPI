namespace VpServiceAPI.Entities.Statistics
{
    public class TimeStatistic : BaseStatistic
    {
        public int[] Substituted { get; set; }
        public int[] Missed { get; set; }

        public TimeStatistic(string name, int[] substituted, int[] missed) : base(name)
        {
            Substituted = substituted;
            Missed = missed;
        }
    }
}
