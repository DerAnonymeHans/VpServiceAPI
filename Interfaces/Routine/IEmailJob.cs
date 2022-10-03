using VpServiceAPI.Entities.Notification;

namespace VpServiceAPI.Interfaces
{
    public interface IEmailJob
    {
        public void Send(Email notification, string reason = "NEWPLAN");
    }
}
