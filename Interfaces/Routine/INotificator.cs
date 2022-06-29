using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface INotificator
    {
        public void Notify(Notification notification);
    }
}
