using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface IEmailBuilder
    {
        public Notification Build(NotificationBody notificationBody, string receiver);
        public void ChangeTemplate(string path);
    }
}
