namespace VpServiceAPI.Entities.Statistics
{
    public sealed class KeyCountStatistic
    {
        public string Key { get; set; } = "";
        public int Count { get; set; }
        public string? Type { get; set; }
    }
}
