using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using VpServiceAPI.Entities.Tools;
using VpServiceAPI.Enums;

namespace VpServiceAPI.Entities.Lernsax
{
    public sealed record Lernsax
    {
        public LernsaxService[] Services { get; set; }
        public LernsaxCredentials? Credentials { get; set; }
        public List<LernsaxMail>? Mails { get; set; }
        public StrictDateTime LastMailDateTime { get; } = new("s", new DateTime(2005, 12, 2, 11, 55, 0));

        public Lernsax(LernsaxService[] services)
        {
            Services = services;
        }

        public Lernsax(LernsaxService[] services, LernsaxCredentials credentials)
        {
            Services = services;
            Credentials = credentials;
        }   

    }        
}
