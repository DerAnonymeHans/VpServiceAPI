using VpServiceAPI.Entities.Notification;

namespace VpServiceAPI.Interfaces
{
    public interface IEmailBuilder
    {
        public Email Build(NotificationBody notificationBody, string receiver, string? template = null);
        public string BuildGradeBody(NotificationBody notificationBody);
        public void ChangeTemplate(string path);
    }
}
