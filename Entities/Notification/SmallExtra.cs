namespace VpServiceAPI.Entities.Notification
{
    public record SmallExtra
    {
        public string Text { get; init; } = "";
        public string Author { get; set; } = "";
    }
}
