using System.Threading.Tasks;
using VpServiceAPI.Entities.Persons;

namespace VpServiceAPI.Interfaces
{
    public interface IPushJob
    {
        public Task Push(User user, IGlobalNotificationBody globalBody, IGradeNotificationBody gradeBody);
    }
}
