using System;

namespace VpServiceAPI.Entities.Notification
{
    public record Email(string Receiver, string Subject, string Body)
    {
        public string Sender { get; set; } = Environment.GetEnvironmentVariable("SMTP_USER") ?? "vp.mailservice.kepler@gmail.com";
    }
}
