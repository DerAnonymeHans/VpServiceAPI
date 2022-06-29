using System;
using System.Net;
using System.Net.Mail;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.Notification
{
    public class TestNotificator : INotificator
    {
        private readonly IMyLogger Logger;

        public TestNotificator(IMyLogger logger)
        {
            Logger = logger;
        }
        public void Notify(Entities.Notification Notification)
        {
            Logger.Info(LogArea.Notification, $"Would have sended Email to {Notification.Receiver}.");
            if(Environment.GetEnvironmentVariable("MODE") == "Testing") Logger.Debug(Notification.Body);
        }
    }

    public class ProdNotificator : INotificator
    {
        private readonly IMyLogger Logger;
        static private readonly string SmtpAddress = "smtp.gmail.com";
        static private readonly int SmtpPort= 587;
        static private readonly bool EnableSSL = true;
        public ProdNotificator(IMyLogger logger)
        {
            Logger = logger;
        }
        public void Notify(Entities.Notification notification)
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

                smtp.Send(mail);
                Logger.Info(LogArea.Notification, "Send Email to: " + notification.Receiver);

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
