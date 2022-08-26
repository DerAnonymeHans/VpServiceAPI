using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Enums;
using VpServiceAPI.Repositories;

namespace VpServiceAPI.Interfaces
{
    public interface IUserRepository
    {
        public Task<bool> UserExists(string mail, UserStatus status = UserStatus.NORMAL);
        public Task<User> GetUser(string mail);
        public Task<List<User>> GetUsers(UserStatus status= UserStatus.NORMAL);
        public Task<User> ValidateUser(string name, string mail, string grade, string mode);
        public Task AddUserRequest(User user);
        public Task AcceptUser(string mail);
        public Task RejectUser(string mail);

        public Task<User> TryGetAuthenticatedUserFromRequest(IRequestCookieCollection cookies);
        public Task<bool> IsAuthenticated(string mail, string mailHash);
        public string GetAuthenticationHash(string mail);
        public Task<string> StartHashResetAndGetKey(string mail);
        public Task<MailHashPair> EndHashResetAndGetMailHashPair(string key);
        public Task SendHashResetMail(string mail, string? linkTo = null);
    }
}
