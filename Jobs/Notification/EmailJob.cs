using System;
using System.Net;
using System.Net.Mail;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.Notification
{
    public sealed class TestEmailJob : IEmailJob
    {
        private readonly IMyLogger Logger;

        public TestEmailJob(IMyLogger logger)
        {
            Logger = logger;
        }
        public void Send(Entities.Notification.Email Notification, string reason = "NEWPLAN")
        {
            Logger.Info(LogArea.Notification, $"{reason}: Would have sended Email to {Notification.Receiver}.");
            //if(Environment.GetEnvironmentVariable("MODE") == "Testing") Logger.Debug(Notification.Body);
        }
    }

    public sealed class ProdEmailJob : IEmailJob
    {
        private readonly IMyLogger Logger;
        static private readonly string SmtpAddress = "smtp.gmail.com";
        static private readonly int SmtpPort= 587;
        static private readonly bool EnableSSL = true;
        public ProdEmailJob(IMyLogger logger)
        {
            Logger = logger;
        }
        public void Send(Entities.Notification.Email notification, string reason = "NEWPLAN")
        {
            try
            {
                var mail = new MailMessage();
                var smtp = new SmtpClient();

                mail.From = new MailAddress(notification.Sender);
                mail.To.Add(new MailAddress(notification.Receiver));
                mail.Subject = notification.Subject;
                mail.Body = notification.Body;
                mail.IsBodyHtml = true;

                smtp.Host = SmtpAddress;
                smtp.EnableSsl = EnableSSL;
                smtp.Port = SmtpPort;
                smtp.Credentials = new NetworkCredential(notification.Sender, Environment.GetEnvironmentVariable("SMTP_PW"));

                

                smtp.SendAsync(mail, null);
                Logger.Info(LogArea.Notification, $"{reason}: Send Email to: " + notification.Receiver);

            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to send email.", notification);
                return;
            }
            return;
        }
    }

}
