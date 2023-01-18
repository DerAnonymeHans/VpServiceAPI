using AE.Net.Mail;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Lernsax;
using VpServiceAPI.Entities.Notification;
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
        private const int MAIL_COUNT = 30;
        private const string SMTP_ADDRESS = "mail.lernsax.de";
        private const string IMAP_ADDRESS = "mail.lernsax.de";


        public LernsaxMailService(IMyLogger logger, IDataQueries dataQueries, IUserRepository userRepository, IPushJob pushJob)
        {
            Logger = logger;
            DataQueries = dataQueries;
            UserRepository = userRepository;
            PushJob = pushJob;
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
                if(!IsNewMail(user.Lernsax, out LernsaxMail? newestMail)) return;
                user.Lernsax.LastMailDateTime.Set(newestMail!.DateTime);
                await UserRepository.Lernsax.SetLernsax(user, new[] { LernsaxData.LAST_MAIL_DATETIME });
                await NotifyUser(user.User, newestMail!);
            }catch(Exception ex)
            {
                Logger.Error(LogArea.LernsaxMail, ex, "Tried to run mail service on user: " + user.User.Address);
            }
        }

        private ImapClient GenerateImapClient(LernsaxCredentials creds)
        {
            return new ImapClient(IMAP_ADDRESS, creds.Address, creds.Password, AuthMethods.Login, 993, true);
        }

        private bool IsNewMail(Entities.Lernsax.Lernsax lernsax, out LernsaxMail? newestMail)
        {
            newestMail = null;
            if (lernsax.Credentials is null) throw new AppException("Die Emails können nur bei angegebenen Anmeldedaten geladen werden.");
            using (ImapClient ic = GenerateImapClient(lernsax.Credentials))
            {
                ic.SelectMailbox("INBOX");
                var messageCount = ic.GetMessageCount();
                var _newestMail = ic.GetMessage(messageCount - 1);
                if(_newestMail.Date > lernsax.LastMailDateTime.DateTime)
                {
                    newestMail = new LernsaxMail();
                    newestMail.Body = _newestMail.Body;
                    newestMail.DateTime = _newestMail.Date;
                    newestMail.Subject = _newestMail.Subject;
                    newestMail.SenderDisplayName = _newestMail.From.DisplayName;
                    newestMail.Sender = _newestMail.From.Address;
                    
                    return true;
                }
                return false;
            }
        }
        public LernsaxMail[] GetMails(LernsaxCredentials creds, bool headersOnly = true, int maxCount = MAIL_COUNT)
        {
            using (ImapClient ic = GenerateImapClient(creds))
            {
                ic.SelectMailbox("INBOX");
                var messageCount = ic.GetMessageCount();
                var rawMsgs = ic.GetMessages(Math.Max(0, messageCount - maxCount), Math.Max(0, messageCount - 1), headersOnly);
                return rawMsgs.Select(mail =>
                {
                    return new LernsaxMail
                    {
                        DateTime = mail.Date,
                        Sender = mail.From.Address,
                        SenderDisplayName = mail.From.DisplayName,
                        Subject = mail.Subject,
                        Body = mail.Body,
                    };
                }).ToArray();
            }
        }
        public string GetMailBody(LernsaxCredentials creds, string mailId)
        {
            var headers = GetMails(creds, false, MAIL_COUNT + 1);
            var selectedMailIdx = Array.FindIndex(headers, mail => mail.Id == mailId);
            if (selectedMailIdx == -1) throw new AppException("Die Email wurde nicht gefunden.");
            using (ImapClient ic = GenerateImapClient(creds))
            {
                ic.SelectMailbox("INBOX");
                var messageCount = ic.GetMessageCount();
                Logger.Debug(messageCount - selectedMailIdx);
                return ic.GetMessage(messageCount - (headers.Length - selectedMailIdx)).Body;
            }
            
        }
        private async Task NotifyUser(User user, LernsaxMail mail)
        {
            var pushOptions = new PushOptions("Neue Lernsax Email", $"{mail.SenderDisplayName.CutToLength(16)}: {mail.Subject.CutToLength(60)}")
            {
                Icon = $"{Environment.GetEnvironmentVariable("URL")}/api/Notification/Logo.png",
                Badge = $"{Environment.GetEnvironmentVariable("URL")}/api/Notification/Badge_LS.png",
                Data = new PushData(user.Name, "/Benachrichtigung?page=lernsax&action=email")
            };
            await PushJob.Push(user, pushOptions, "LSMAIL");
        }

        public void SendMail(LernsaxCredentials creds, Email email)
        {
            var mail = new System.Net.Mail.MailMessage();
            var smtp = new SmtpClient();

            mail.From = new MailAddress(email.Sender);
            mail.To.Add(new MailAddress(email.Receiver));
            mail.Subject = email.Subject;
            mail.Body = email.Body;
            mail.IsBodyHtml = true;

            smtp.Host = SMTP_ADDRESS;
            smtp.EnableSsl = true;
            smtp.Port = 587;
            smtp.Credentials = new NetworkCredential(creds.Address, creds.Password);
            Logger.Debug(creds);
            Logger.Debug(email);
            smtp.Send(mail);
        }
    }
}
