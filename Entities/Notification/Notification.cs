using System;

namespace VpServiceAPI.Entities.Notification
{
    public record Email
    {
        public string Sender { get; set; } = Environment.GetEnvironmentVariable("SMTP_USER") ?? "vp.mailservice.kepler@gmail.com";
        public string Receiver { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
    }
}
