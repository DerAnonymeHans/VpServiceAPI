using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface INotificationBuilder
    {
        public Notification Build(NotificationBody notificationBody, string receiver);
        public void ChangeTemplate(string path);
    }
}
