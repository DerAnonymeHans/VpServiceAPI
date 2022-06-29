namespace VpServiceAPI.Entities
{
    public record NotificationRow
    {
        public bool HasChange { get; init; }
        public PlanRow Row { get; init; } = new();
    }
}
