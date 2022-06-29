using System.Collections.Generic;

namespace VpServiceAPI.Entities
{
    public record GradeInfo
    {
        public string Grade { get; init; } = "";
        public bool IsAffected { get; init; } = false;
        public List<NotificationRow>? Table { get; init; } = new();
    }
}
