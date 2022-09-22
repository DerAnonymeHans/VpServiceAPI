using VpServiceAPI.Entities.Notification;

namespace VpServiceAPI.Interfaces
{
    public interface IEmailBuilder
    {
        public Notification Build(NotificationBody notificationBody, string receiver, string? template = null);
        public string BuildGradeBody(NotificationBody notificationBody);
        public void ChangeTemplate(string path);
    }
}
