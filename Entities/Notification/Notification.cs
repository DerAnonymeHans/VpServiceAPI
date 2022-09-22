using System;

namespace VpServiceAPI.Entities.Notification
{
    public record Notification
    {
        public readonly string Sender = Environment.GetEnvironmentVariable("SMTP_USER") ?? "vp.mailservice.kepler@gmail.com";
        public string Receiver { get; set; } = "";
        public string? Subject { get; set; }
        public string Body { get; set; } = "";
    }
}
