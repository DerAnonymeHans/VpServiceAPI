using System;
using VpServiceAPI.TypeExtensions.String;

namespace VpServiceAPI.Entities.Lernsax
{
    public record LernsaxMailHead
    {
        public string Subject { get; set; } = "";
        public string Sender { get; set; } = "";
        public string SenderDisplayName { get; set; } = "";
        public DateTime DateTime { get; set; }
        public string Id => (DateTime.ToString("s") + Sender.CutToLength(4) + Subject.CutToLength(8)).TrimEnd();
    }
    public sealed record LernsaxMail : LernsaxMailHead
    {        
        public string Body { get; set; } = "";

        public override string ToString()
        {
            return $"{DateTime:dd.MM.yyyy HH:mm}: {Sender}---{Subject}--->{Body[..20]}";
        }
    }

    public sealed record LernsaxMailToSend(string Receiver, string Subject, string Body);
}
