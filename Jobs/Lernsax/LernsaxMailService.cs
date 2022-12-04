using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Lernsax;
using VpServiceAPI.Entities.Persons;
using VpServiceAPI.Entities.Tools;
using VpServiceAPI.Enums;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Interfaces.Lernsax;
using VpServiceAPI.Jobs.Notification;
using VpServiceAPI.Tools;
using VpServiceAPI.TypeExtensions.String;

namespace VpServiceAPI.Jobs.Lernsax
{
    public class LernsaxMailService : ILernsaxMailService
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private readonly IUserRepository UserRepository;
        private readonly IPushJob PushJob;

        public LernsaxMailService(IMyLogger logger, IDataQueries dataQueries, IUserRepository userRepository, IPushJob pushJob)
        {
            Logger = logger;
            DataQueries = dataQueries;
            UserRepository = userRepository;
            PushJob = pushJob;
        }
        public async Task<List<LernsaxMail>?> GetStoredEmails(User user)
        {
            return await UserRepository.Lernsax.GetMails(user);
        }
        public async Task RunOnUser(User user)
        {
            await RunOnUser(new UserWithLernsax(user, await UserRepository.Lernsax.GetLernsaxForEmailChecking(user)));
        }
        public async Task RunOnUser(UserWithLernsax user)
        {
            try
            {
                user.Lernsax.LastMailDateTime.Set(await UserRepository.Lernsax.GetLastMailDatetime(user.User));
                var res = await GetMailsIfUpdate(user.Lernsax);
                if (res is null) return;
                user.Lernsax.Mails = res;
                user.Lernsax.LastMailDateTime.Set(user.Lernsax.Mails[0].DateTime);
                await StoreEmails(user.User, user.Lernsax);
                await NotifyUser(user.User, user.Lernsax.Mails[0]);
            }catch(Exception ex)
            {
                Logger.Error(LogArea.LernsaxMail, ex, "Tried to run mail service on user: " + user.User.Address);
            }
        }

        public async Task<List<LernsaxMail>?> GetMailsIfUpdate(Entities.Lernsax.Lernsax lernsax)
        {
            using var client = new HttpClient()
            {
                BaseAddress = new Uri("https://www.lernsax.de")
            };

            string mailOverviewHtml = await GetMailOverviewHtml(lernsax.Credentials, client);
            var wrapper = await ExtractMailsAndCheckForUpdate(lernsax.LastMailDateTime, mailOverviewHtml, client);
            if (wrapper.Status == UpdateCheckStatus.NOT_NEW) return null;
            if (wrapper.Body is null) throw new AppException("Wrapper body is null even though status is null");

            return wrapper.Body;
           
        }
        
        
        private async Task<string> GetMailOverviewHtml(LernsaxCredentials? credentials, HttpClient client)
        {
            string html = await UserRepository.Lernsax.Login(credentials, client);
            string sid = html.SubstringSurroundedBy(@"<a href=""105592.php?sid=", @""">") ?? throw new Exception("sid for mail page not found");

            html = await new StreamReader((await client.GetAsync($"wws/105592.php?sid={sid}")).Content.ReadAsStream(), Encoding.Latin1).ReadToEndAsync();
            html = html[html.IndexOf(@"<form action=""/wws/105592.php?")..];
            html = html[html.IndexOf(@"<div class=""content_outer"" id=""content"">")..];
            html = html[html.IndexOf(@"<div class=""jail_table"">")..];
            html = html[html.IndexOf(@"<tbody>")..html.IndexOf("</tbody>")];
            return html;
        }             
       
        
        // returns Status.FAIL if no update and SUCCESS for new mail
        private async Task<StatusWrapper<UpdateCheckStatus, List<LernsaxMail>?>> ExtractMailsAndCheckForUpdate(StrictDateTime lastMailDateTime, string html, HttpClient client)
        {
            var mails = new List<LernsaxMail>();
            for(int i=0; i<20; i++)
            {
                var row = XMLParser.GetNode(html, "tr");
                if (row is null) break;

                (LernsaxMail mail, string bodyPath) = ExtractMailHead(row);

                // no new mail
                if(i == 0 && lastMailDateTime >= mail.DateTime && Environment.GetEnvironmentVariable("MODE") != "Testing")
                {
                    return new(UpdateCheckStatus.NOT_NEW, null);
                }

                var body = await GetMailBody(bodyPath, client);
                mail.Body = body;
                mails.Add(mail);

                html = html[row.Length..];
            }

            return new(UpdateCheckStatus.IS_NEW, mails);
        }  
        private (LernsaxMail mailHead, string bodyPath) ExtractMailHead(string rowHtml)
        {
            var cell = rowHtml.SubstringSurroundedBy(@"c_subj"">", "</td>") ?? "";
            var subject = XMLParser.GetNodeContent(cell, "a");
            var path = cell.SubstringSurroundedBy(@"data-popup=""", @"""");

            cell = rowHtml.SubstringSurroundedBy(@"c_from"">", "</td>") ?? "";
            var sender = XMLParser.GetNodeContent(cell, "span");

            cell = rowHtml.SubstringSurroundedBy(@"c_date""", "/td>") ?? "";
            var date = cell.SubstringSurroundedBy(">", "<");

            if (subject is null || path is null || sender is null || date is null)
            {
                throw new AppException($"Cannnot extract mail. Subject: {subject}, Path: {path}, Sender: {sender}, Date: {date}");
            }

            DateTime dateTime = DateTime.ParseExact(date, "dd.MM.yyyy HH:mm", null);

            return (new LernsaxMail
            {
                Subject = subject,
                Sender = sender,
                DateTime = dateTime
            }, path);
        }


        private async Task<string> GetMailBody(string path, HttpClient httpClient)
        {
            path = path.Replace("amp;", "");
            var html = await new StreamReader((await httpClient.GetAsync($"/wws/{path}")).Content.ReadAsStream(), Encoding.Latin1).ReadToEndAsync();
            string startTag = @"<p class=""panel"">";
            int startIdx = html.IndexOf(startTag) + startTag.Length;
            html = html[startIdx..html.IndexOf("</p>", startIdx)];
            return html;
        }
        
        private async Task StoreEmails(User user, Entities.Lernsax.Lernsax lernsax)
        {
            await UserRepository.Lernsax.SetLernsax(user, lernsax, new LernsaxData[] { LernsaxData.MAILS, LernsaxData.LAST_MAIL_DATETIME });
        }
        private async Task NotifyUser(User user, LernsaxMail mail)
        {
            var pushOptions = new PushOptions("Neue Lernsax Email", $"{mail.Sender.CutToLength(16)}..: {mail.Subject.CutToLength(60)}")
            {
                Icon = $"{Environment.GetEnvironmentVariable("URL")}/api/Notification/Logo.png",
                Badge = $"{Environment.GetEnvironmentVariable("URL")}/api/Notification/Badge_LS.png",
                Data = new PushData(user.Name, "/Benachrichtigung?page=lernsax&action=email")
            };
            await PushJob.Push(user, pushOptions, "LSMAIL");
        }

        
    }
}
