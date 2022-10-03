using System.Threading.Tasks;
using VpServiceAPI.Entities.Persons;
using VpServiceAPI.Jobs.Notification;

namespace VpServiceAPI.Interfaces
{
    public interface IPushJob
    {
        public Task Push(User user, PushOptions pushOptions, string reason="NEWPLAN");
    }
}
