using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Lernsax;
using VpServiceAPI.Entities.Persons;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Interfaces.Lernsax
{
    public interface ILernsaxRepository
    {
        public Task<LernsaxService[]> GetServices(User user);
        public Task<LernsaxCredentials?> GetCredentials(User user);
        public Task<string> GetLastMailDatetime(User user);
        public Task<List<LernsaxMail>?> GetMails(User user);
        public Task<Entities.Lernsax.Lernsax> GetLernsax(User user);

        public Task<Entities.Lernsax.Lernsax> GetLernsaxForEmailChecking(User user);

        public Task SetLernsax(User user, Entities.Lernsax.Lernsax lernsax, LernsaxData[] whatData);
        public Task SetLernsax(UserWithLernsax user, LernsaxData[] whatData);

        public Task<List<UserWithLernsax>> GetUsersWithLernsaxServices();

        public Task<string> Login(User user, HttpClient? client);
        public Task<string> Login(LernsaxCredentials credentials, HttpClient? client);
    }
}
