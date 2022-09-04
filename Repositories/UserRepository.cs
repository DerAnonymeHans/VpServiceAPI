using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using VpServiceAPI.Entities;
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

namespace VpServiceAPI.Repositories
{
    public class TestUserRepository : IUserRepository
    {
        private List<User> Users { get; init; }

        public TestUserRepository()
        {
            Users = new()
            {
                new User("Pascal", "pascal.setzer@gmail.com", "11", UserStatus.NORMAL.ToString(), NotifyMode.PWA.ToString(), "", "27221354")
            };
        }

        public async Task AddUserRequest(User user)
        {
            Users.Add(user);
        }

        public async Task<User?> GetUser(string mail)
        {
            return Users.Find((user) => user.Address == mail);
        }

        public async Task<List<User>> GetUsers(UserStatus status= UserStatus.NORMAL)
        {
            return Users.FindAll((user) => user.Status == status);
        }

        public async Task<User> ValidateUser(string? name, string? mail, string? grade)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new AppException("Der Name darf nicht leer sein.");
            if (name.Length < 2) throw new AppException("Der Name muss mindestens 2 Zeichen lang sein.");
            if (name.Length > 20) throw new AppException("Der Name darf nicht länger als 20 Zeichen lang sein.");
            var matches = new Regex(@"[^a-zA-ZäöüÄÖÜß\s]").Matches(name);
            if (matches.Count > 0) throw new AppException($"Der Name beinhaltet folgende nicht erlaubte Zeichen: '{string.Join(", ", matches.Cast<Match>().Select(m => m.Value))}'");

            if(string.IsNullOrWhiteSpace(mail)) throw new AppException("Die Email darf nicht leer sein.");
            try
            {
                new MailAddress(mail);
            }
            catch (FormatException)
            {
                throw new AppException("Bitte gib eine valide Email Addresse an");
            }

            if (string.IsNullOrWhiteSpace(grade)) throw new AppException("Die Klassenstufe darf nicht leer sein.");
            string[] grades = new[] { "5", "6", "7", "8", "9", "10", "11", "12" };
            if (Array.IndexOf(grades, grade) == -1) throw new AppException("Bitte gib eine valide Klassenstufe an (5-12)");

            return new User(name, mail, grade, UserStatus.REQUEST.ToString(), "", "", "");
        }

        public Task AcceptUser(string mail)
        {
            throw new NotImplementedException();
        }

        public Task RejectUser(string mail)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UserExists(string mail, UserStatus status = UserStatus.NORMAL)
        {
            return Users.Exists((user) => user.Address == mail);
        }

        public async Task<bool> IsAuthenticated(string mail, string mailHash)
        {
            if (!await UserExists(mail))
            {
                return false;
            };
            var encoded = WebUtility.UrlDecode(mailHash).Replace(' ', '+');
            return StringCipher.Decrypt(encoded) == Convert.ToBase64String(Encoding.UTF8.GetBytes(mail));
        }

        public string GetAuthenticationHash(string mail)
        {
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(mail));
            var hash = StringCipher.Encrypt(base64);
            return hash;
        }

        public async Task<string> StartHashResetAndGetKey(string mail)
        {
            if (!await UserExists(mail)) throw new AppException("Das Schlüssel kann nicht ausgestellt werden, da die Email Addresse nicht existiert.");
            var randomBytes = new byte[4]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }
            var stringBuilder = new StringBuilder();
            foreach (var _byte in randomBytes)
            {
                stringBuilder.Append(_byte.ToString("X2"));
            }
            string key = stringBuilder.ToString();
            string timestamp = DateTime.Now.Ticks.ToString();
            return key;
        }

        public Task<MailHashPair> EndHashResetAndGetMailHashPair(string key)
        {
            throw new NotImplementedException();
        }

        public Task<User> ValidateUser(string name, string mail, string grade, string mode)
        {
            throw new NotImplementedException();
        }

        public Task<User> TryGetAuthenticatedUserFromRequest(IRequestCookieCollection cookies)
        {
            throw new NotImplementedException();
        }

        public Task SendHashResetMail(string mail)
        {
            throw new NotImplementedException();
        }
    }

    public class ProdUserRepository : IUserRepository
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private readonly IEmailJob EmailJob;

        private const int MAX_RESET_DURA = 1000 * 60 * 10; // 10min
        public ProdUserRepository(IMyLogger logger, IDataQueries dataQueries, IEmailJob emailJob)
        {
            Logger = logger;
            DataQueries = dataQueries;
            EmailJob = emailJob;
        }

        public async Task<bool> UserExists(string mail, UserStatus status = UserStatus.NORMAL)
        {
            return (await DataQueries.Load<string, dynamic>("SELECT id FROM `users` WHERE address=@mail AND status=@status", new { mail, status=status.ToString() })).Count == 1;
        }
        public async Task<User?> GetUser(string mail)        {

            var res = await DataQueries.Load<User, dynamic>("SELECT name, address, grade, status, mode, sub_day, push_id FROM `users` WHERE address=@mail", new { mail });
            if (res.Count == 0) return null;
            return res[0];
        }
        public async Task<List<User>> GetUsers(UserStatus status = UserStatus.NORMAL)
        {
            return await DataQueries.Load<User, dynamic>("SELECT name, address, grade, status, mode, sub_day, push_id FROM `users` WHERE status=@status ORDER BY `grade`, `mode`", new { status = status.ToString() });
        }
        public async Task<User> ValidateUser(string name, string mail, string grade, string mode)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new AppException("Der Name darf nicht leer sein.");
            if (string.IsNullOrWhiteSpace(mail)) throw new AppException("Die Email darf nicht leer sein.");
            if (string.IsNullOrWhiteSpace(grade)) throw new AppException("Die Klassenstufe darf nicht leer sein.");

            try
            {
                AttackDetector.Detect(name, mail, grade);
            }catch(AppException ex)
            {
                Logger.Warn(LogArea.Attack, ex, "Somebody tried to attack on user validation");
                throw new AppException(ex.Message);
            }

            if (name.Length < 2) throw new AppException("Der Name muss mindestens 2 Zeichen lang sein.");
            if (name.Length > 20) throw new AppException("Der Name darf nicht länger als 20 Zeichen lang sein.");
            var matches = new Regex(@"[^a-zA-ZäöüÄÖÜß\s]").Matches(name);
            if (matches.Count > 0) throw new AppException($"Der Name beinhaltet folgende nicht erlaubte Zeichen: '{string.Join(", ", matches.Cast<Match>().Select(m => m.Value))}'");

            try
            {
                new MailAddress(mail);                
            }
            catch (FormatException)
            {
                throw new AppException("Bitte gib eine valide Email Addresse an");
            }

            string[] grades = new[] { "5", "6", "7", "8", "9", "10", "11", "12" };
            if (Array.IndexOf(grades, grade) == -1) throw new AppException("Bitte gib eine valide Klassenstufe an (5-12)");

            var res = (await DataQueries.Load<int, dynamic>("SELECT id FROM users WHERE address=@address", new { address = mail }));
            if(res.Count != 0) throw new AppException("Die Email Addresse ist bereits in den Verteiler aufgenommen und kann nicht nochmals hinzugefügt werden.");

            var _mode = mode switch
            {
                "pwa" => NotifyMode.PWA,
                "mail" => NotifyMode.EMAIL,
                _ => throw new AppException("Bitte gib einen validen Benachrichtigungsweg an.")
            };

            return new User(name, mail, grade, UserStatus.REQUEST.ToString(), _mode.ToString(), "", "");
        }
        public async Task AddUserRequest(User user)
        {
            await DataQueries.Save("INSERT INTO users(name, address, grade, status, mode, sub_day) VALUES (@name, @address, @grade, @status, @mode, @date)", new { name = user.Name, address = user.Address, grade = user.Grade, date = DateTime.Now.ToString("dd.MM.yyyy"), status = UserStatus.REQUEST.ToString(), mode=user.NotifyMode.ToString() });
        }
        public async Task AcceptUser(string mail)
        {
            await DataQueries.Save("UPDATE users SET status=@status WHERE address=@address", new { address = mail, status=UserStatus.NORMAL.ToString() });
            var user = await GetUser(mail);
            if (user is null) throw new AppException("Cannot accept user who is not existing");
            await DataQueries.AddUserToBackupDB(user);
            if(user.NotifyMode == NotifyMode.PWA)
            {
                await SendHashResetMail(mail);
            }
        }
        public async Task RejectUser(string mail)
        {
            await DataQueries.Delete("DELETE FROM users WHERE address=@address AND status=@requestStatus", new { address = mail, requestStatus=UserStatus.REQUEST.ToString() }); ;
        }



        public async Task<User> TryGetAuthenticatedUserFromRequest(IRequestCookieCollection cookies)
        {
            bool isLoggedIn = cookies.TryGetValue("userAuthMail", out string? userAuthMail);
            isLoggedIn = cookies.TryGetValue("userAuthHash", out string? userAuthHash) && isLoggedIn;
            isLoggedIn = isLoggedIn && userAuthMail is not null && userAuthHash is not null;
            if (isLoggedIn)
            {
                if(await IsAuthenticated(userAuthMail ?? "", userAuthHash ?? ""))
                {
#pragma warning disable CS8603 // Possible null reference return.
                    return await GetUser(userAuthMail ?? "");
#pragma warning restore CS8603 // Possible null reference return.
                };
            }
            throw new AppException("Du musst angemeldet sein um diese Daten zu sehen.");
        }
        public async Task<bool> IsAuthenticated(string mail, string mailHash)
        {
            if (!await UserExists(mail))
            {
                return false;
            };
            var encoded = WebUtility.UrlDecode(mailHash).Replace(' ', '+');
            return StringCipher.Decrypt(encoded) == Convert.ToBase64String(Encoding.UTF8.GetBytes(mail));
        }

        public string GetAuthenticationHash(string mail)
        {
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(mail));
            var hash = StringCipher.Encrypt(base64);
            return hash;
        }

        public async Task<string> StartHashResetAndGetKey(string mail)
        {
            if (!await UserExists(mail)) throw new AppException("Das Schlüssel kann nicht ausgestellt werden, da die Email Addresse nicht existiert.");
            var randomBytes = new byte[8]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }
            var stringBuilder = new StringBuilder();
            foreach(var _byte in randomBytes)
            {
                stringBuilder.Append(_byte.ToString("X2"));
            }
            string key = stringBuilder.ToString();
            await DataQueries.Save("UPDATE users SET reset_key=@resetKey WHERE address=@mail", new { mail, resetKey = key });
            return key;
        }
        public async Task<MailHashPair> EndHashResetAndGetMailHashPair(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new AppException("Der Code ist ungültig. Er darf nicht leer sein.");

            var rows = await DataQueries.Load<KeyMailHelper, dynamic>("SELECT reset_key, address FROM users WHERE reset_key = @key", new { key });
            if (rows.Count == 0) throw new AppException("Der Code ist ungültig.");

            await DataQueries.Save("UPDATE users SET reset_key='' WHERE address=@mail", new { mail = rows[0].Mail });
            return new MailHashPair(rows[0].Mail, GetAuthenticationHash(rows[0].Mail));
        }

        public async Task SendHashResetMail(string mail)
        {
            string linkTo = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" ? $"{Environment.GetEnvironmentVariable("CLIENT_URL")}" : "http://localhost:3000";
            
            EmailJob.Send(new Entities.Notification
            {
                Body = await GenerateBody(mail, linkTo),
                Receiver = mail,
                Subject = "Anmeldung bei Kepleraner"
            });
        }

        private async Task<string> GenerateBody(string mail, string linkTo)
        {            
            string key = await StartHashResetAndGetKey(mail);
            var user = await GetUser(mail);
            if (user is null) throw new AppException("Cannot generateMail Body for not existing user");
            return string.Join("<br>", new string[]
            {
                $"<h1>Hallo {user.Name}!</h1>",
                "<p>Mit Hilfe dieser Mail wirst du bei Kepleraner angemeldet. Dafür musst du nur auf folgenden Link drücken:.</p>",
                $@"<h2><a href=""{linkTo}/Benachrichtigung?code={key}"">Diesen Link drücken</a></h2>"
            });
        }

    }

    public class KeyMailHelper
    {
        public string ResetKey { get; init; }
        public string Mail { get; init; }
        public KeyMailHelper(string reset_key, string address)
        {
            ResetKey = reset_key;
            Mail = address;
        }
    }

    public class MailHashPair
    {
        public MailHashPair(string mail, string hash)
        {
            Mail = mail;
            Hash = hash;
        }

        public string Mail { get; init; }
        public string Hash { get; init; }
    }
}
