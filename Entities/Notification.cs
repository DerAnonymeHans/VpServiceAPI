using System;

namespace VpServiceAPI.Entities
{
    public record Notification
    {
        public readonly string Sender = Environment.GetEnvironmentVariable("SMTP_USER");
        public string Receiver { get; set; } = "";
        public string? Subject { get; set; }
        public string Body { get; set; } = "";
    }
}
