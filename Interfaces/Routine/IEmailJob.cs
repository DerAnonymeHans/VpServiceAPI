using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface IEmailJob
    {
        public void Send(Notification notification);
    }
}
