using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;
using System.Net.Mail;
using System;
using VpServiceAPI.Tools;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using VpServiceAPI.Enums;
using Microsoft.AspNetCore.Http;
using VpServiceAPI.Entities.Persons;
using VpServiceAPI.Entities.Lernsax;
using VpServiceAPI.Interfaces.Lernsax;

namespace VpServiceAPI.Repositories
{
    public sealed class TestUserRepository : IUserRepository
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private readonly IEmailJob EmailJob;
        private readonly ProdUserRepository prodRepository;
        private string[] TestUserMails = new[] { "pascal.setzer@gmail.com"/*, "deltass@web.de", "pia.setzer@gmail.com"*/ };

        public ILernsaxRepository Lernsax { get; init; }

        public TestUserRepository(IMyLogger logger, IDataQueries dataQueries, IEmailJob emailJob, ILernsaxRepository lernsax)
        {
            Logger = logger;
            DataQueries = dataQueries;
            EmailJob = emailJob;
            Lernsax = lernsax;
            prodRepository = new(logger, dataQueries, emailJob, lernsax);
        }

        public async Task<bool> UserExists(string mail, UserStatus? status = UserStatus.NORMAL)
        {
            string whereStatus = status is null ? "" : "AND status=@status";
            var existsInDB = (await DataQueries.Load<string, dynamic>($"SELECT id FROM `users` WHERE address=@mail {whereStatus}", new { mail, status = status.ToString() })).Count == 1;
            return existsInDB && TestUserMails.Contains(mail);
        }
        public async Task<User?> GetUser(string mail)
        {
            if (!TestUserMails.Contains(mail)) return null;

            return await prodRepository.GetUser(mail);
        }
        public async Task<List<User>> GetUsers(UserStatus status = UserStatus.NORMAL)
        {
            var users = await prodRepository.GetUsers(status);
            users = users.FindAll(user => TestUserMails.Contains(user.Address));
            return users;
        }
        public async Task<User> ValidateUser(string name, string mail, string grade, string mode)
        {
            return await prodRepository.ValidateUser(name, mail, grade, mode);
        }
        public async Task AddUserRequest(User user)
        {
            await prodRepository.AddUserRequest(user);
        }
        public async Task AcceptUser(string mail)
        {
            if (!TestUserMails.Contains(mail)) return;
            await prodRepository.AcceptUser(mail);
        }
        public async Task RejectUser(string mail)
        {
            await DataQueries.Delete("DELETE FROM users WHERE address=@address AND status=@requestStatus", new { address = mail, requestStatus = UserStatus.REQUEST.ToString() }); ;
        }



        public async Task<User> GetAuthenticatedUserFromRequest(IRequestCookieCollection cookies)
        {
            var user = await prodRepository.GetAuthenticatedUserFromRequest(cookies);
            if (!TestUserMails.Contains(user.Address)) throw new Exception("User not part of test users");
            return user;
        }
        public async Task<bool> IsAuthenticated(string mail, string mailHash)
        {
            if (!TestUserMails.Contains(mail)) throw new Exception("User not part of test users");
            return await prodRepository.IsAuthenticated(mail, mailHash);
        }

        public async Task<string> StartHashResetAndGetKey(string mail)
        {
            if (!TestUserMails.Contains(mail)) throw new Exception("User not part of test users");
            return await prodRepository.StartHashResetAndGetKey(mail);
        }
        public async Task<MailHashPair> EndHashResetAndGetMailHashPair(string key)
        {
            return await prodRepository.EndHashResetAndGetMailHashPair(key);
        }

        public async Task SendHashResetMail(string mail)
        {
            if (!TestUserMails.Contains(mail)) throw new Exception("User not part of test users");
            await prodRepository.SendHashResetMail(mail);
        }


        public Task<UserWithLernsax> GetUserWithLernsax(User user)
        {
            if (!TestUserMails.Contains(user.Address)) throw new Exception("User not part of test users");
            return prodRepository.GetUserWithLernsax(user);
        }

        public async Task<List<UserWithLernsax>> GetUsersWithLernsaxServices()
        {
            var users = await prodRepository.GetUsersWithLernsaxServices();
            return users.FindAll(user => TestUserMails.Contains(user.User.Address));
        }
    }
}
