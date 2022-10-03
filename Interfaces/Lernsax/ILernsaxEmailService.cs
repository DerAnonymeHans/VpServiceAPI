using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Lernsax;
using VpServiceAPI.Entities.Persons;
using VpServiceAPI.Entities.Tools;

namespace VpServiceAPI.Interfaces.Lernsax
{
    public interface ILernsaxMailService
    {
        public Task<List<LernsaxMail>?> GetStoredEmails(User user);
        public Task RunOnUser(UserWithLernsax user);
        public Task RunOnUser(User user);
    }
}
