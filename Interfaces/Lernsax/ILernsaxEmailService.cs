using AE.Net.Mail;
using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Lernsax;
using VpServiceAPI.Entities.Notification;
using VpServiceAPI.Entities.Persons;
using VpServiceAPI.Entities.Tools;

namespace VpServiceAPI.Interfaces.Lernsax
{
    public interface ILernsaxMailService
    {
        public LernsaxMail[] GetMails(LernsaxCredentials creds, bool headersOnly = true, int maxCount = 30);
        public string GetMailBody(LernsaxCredentials creds, string mailId);
        public void SendMail(LernsaxCredentials cred, Email mail);
        public Task RunOnUser(UserWithLernsax user);
        public Task RunOnUser(User user);
    }
}
