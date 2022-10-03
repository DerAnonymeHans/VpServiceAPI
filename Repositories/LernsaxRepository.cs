using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Lernsax;
using VpServiceAPI.Entities.Persons;
using VpServiceAPI.Enums;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Interfaces.Lernsax;
using VpServiceAPI.Tools;
using VpServiceAPI.TypeExtensions.String;

namespace VpServiceAPI.Repositories
{
    public class LernsaxRepository : ILernsaxRepository
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private readonly Dictionary<string, LernsaxService> LernsaxServices = new();

        public LernsaxRepository(IMyLogger logger, IDataQueries dataQueries)
        {
            Logger = logger;
            DataQueries = dataQueries;

            foreach(var service in (LernsaxService[])Enum.GetValues(typeof(LernsaxService))){
                LernsaxServices.Add(service.ToString(), service);
            }
        }

        public async Task<LernsaxCredentials?> GetCredentials(User user)
        {
            var res = await DataQueries.Load<string, dynamic>("SELECT credentials FROM lernsax WHERE userId=@userId", new { userId = user.Id });
            return res[0] is not null ? new(res[0]) : null;
        }

        public async Task<string> GetLastMailDatetime(User user)
        {
            var res = await DataQueries.Load<string, dynamic>("SELECT lastMailDatetime FROM lernsax WHERE userId=@userId", new { userId = user.Id });
            return res[0];
        }

        public async Task<List<LernsaxMail>?> GetMails(User user)
        {
            var res = await DataQueries.Load<string, dynamic>("SELECT mails FROM lernsax WHERE userId=@userId", new { userId = user.Id });
            if (string.IsNullOrWhiteSpace(res[0])) return null;
            return JsonSerializer.Deserialize<List<LernsaxMail>>(StringCipher.Decrypt(res[0], EncryptionKey.LERNSAX));
        }
        public async Task<LernsaxService[]> GetServices(User user)
        {
            var res = await DataQueries.Load<string?, dynamic>("SELECT service FROM lernsax WHERE userId=@userId", new { userId = user.Id });
            if(res is null) return Array.Empty<LernsaxService>();
            if (res[0] is null) return Array.Empty<LernsaxService>();

            var splitted = res[0].Split(",");
            var services = new List<LernsaxService>();
            foreach(var s in splitted)
            {
                if(LernsaxServices.TryGetValue(s, out var service))
                {
                    services.Add(service);
                }
            }
            return services.ToArray();
        }

        public async Task<Lernsax> GetLernsax(User user)
        {
            var lernsax = new Lernsax(await GetServices(user))
            {
                Credentials = await GetCredentials(user),
                Mails = await GetMails(user)
            };
            lernsax.LastMailDateTime.Set(await GetLastMailDatetime(user));
            return lernsax;
        }

        public async Task<Lernsax> GetLernsaxForEmailChecking(User user)
        {
            var lernsax = new Lernsax(await GetServices(user))
            {
                Credentials = await GetCredentials(user),
            };
            lernsax.LastMailDateTime.Set(await GetLastMailDatetime(user));
            return lernsax;
        }

        public async Task SetLernsax(UserWithLernsax user, LernsaxData[] whatData)
        {
            await SetLernsax(user.User, user.Lernsax, whatData);
        }

        public async Task SetLernsax(User user, Lernsax lernsax, LernsaxData[] whatData)
        {
            List<string> toSet = new();
            bool isAll = whatData.Contains(LernsaxData.ALL);
            bool Is(LernsaxData what) => isAll || whatData.Contains(what);

            if (Is(LernsaxData.SERVICES))
            {
                toSet.Add("service=@service");
            }
            if (Is(LernsaxData.CREDENTIALS))
            {
                toSet.Add("credentials=@credentials");
            }
            if (Is(LernsaxData.LAST_MAIL_DATETIME))
            {
                toSet.Add("lastMailDatetime=@lastMailDatetime");
            }
            if (Is(LernsaxData.MAILS))
            {
                toSet.Add("mails=@mails");
            }

            string? encryptedMails = null;
            if (whatData.Contains(LernsaxData.MAILS))
            {
                encryptedMails = StringCipher.Encrypt(JsonSerializer.Serialize(lernsax.Mails), EncryptionKey.LERNSAX);
            }

            await DataQueries.Save($"UPDATE lernsax SET {string.Join(", ", toSet)} WHERE userId=@userId", new
            {
                service = string.Join(',', lernsax.Services.Select(ser => ser.ToString())),
                userId = user.Id,
                credentials = lernsax.Credentials?.Encrypt(),
                lastMailDatetime = lernsax.LastMailDateTime.ToString(),
                mails = encryptedMails
            });
        }

        public async Task<List<UserWithLernsax>> GetUsersWithLernsaxServices()
        {
            var users = await DataQueries.Load<User, dynamic>("SELECT users.id, users.name, users.address, users.grade, users.status, users.mode, users.sub_day, users.push_id, users.push_subscribtion FROM users INNER JOIN lernsax ON users.id = lernsax.userId WHERE lernsax.service != ''", new { });
            var usersWithLernsax = new List<UserWithLernsax>();
            foreach(var user in users)
            {
                usersWithLernsax.Add(new UserWithLernsax(user, new Lernsax(await GetServices(user))
                {
                    Credentials = await GetCredentials(user),
                }));
            }
            return usersWithLernsax;
        }

        public async Task<string> Login(User user, HttpClient? client)
        {
            var creds = await GetCredentials(user);
            if (creds is null) throw new AppException("Der Login in Lernsax ist fehlgeschlagen, da keine Anmeldedaten angegeben wurden.");
            return await Login(creds, client);
        }

        public async Task<string> Login(LernsaxCredentials credentials, HttpClient? client)
        {
            client ??= new HttpClient();
            client.BaseAddress = new Uri("https://www.lernsax.de");

            string html = await client.GetStringAsync("/wws/101505.php");
            string sid = html.SubstringSurroundedBy(@"<a href=""100001.php?sid=", @""" class=""mo"" data-mo=""img_login0"">Anmelden</a>") ?? throw new Exception("Cant find startpage sid");

            // scrape login page and extract sid for POST login
            html = await client.GetStringAsync($"/wws/100001.php?sid={sid}");
            sid = html.SubstringSurroundedBy(@"<form action=""/wws/100001.php?sid=", @""" method=""post"" name=""form0"" id=""form0"" enctype=""multipart/form-data"" onsubmit=""return form_submit();"">") ?? throw new Exception("Cant find loginpage sid");

            // POST login data and get redirected to user start page
            var form = new List<KeyValuePair<string?, string?>>()
            {
                { new("login_nojs", "") },
                { new("login_login", credentials.Address) },
                { new("login_password", credentials.Password) },
                { new("login_submit", "Anmelden") },
                { new("language", "2") }
            };

            FormUrlEncodedContent data = new(form);

            var response = await client.PostAsync($"/wws/100001.php?sid={sid}", data);
            html = new StreamReader(response.Content.ReadAsStream()).ReadToEnd();
            if (!html.Contains("<title>LernSax - ")) throw new AppException("Die von dir angegebenen Anmeldedaten wurden von Lernsax als falsch erkannt.");
            return html;
        }
    }


}
