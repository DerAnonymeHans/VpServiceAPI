using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Jobs.Checking;
using VpServiceAPI.Tools;
using VpServiceAPI.Jobs.Notification;
using System.IO;
using VpServiceAPI.WebResponse;
using VpServiceAPI.Exceptions;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http;
using VpServiceAPI.Enums;
using VpServiceAPI.Entities.Notification;
using System.Collections.Generic;


#nullable enable
namespace VpServiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class BriefController : ControllerBase
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private readonly IEmailJob MailJob;

        private readonly WebResponder WebResponder;

        public BriefController(IMyLogger logger, IDataQueries dataQueries, IEmailJob mailJob)
        {
            Logger = logger;
            DataQueries = dataQueries;
            MailJob = mailJob;

            WebResponder = new("Brief", LogArea.Brief, false, logger);
        }


        [HttpPost]
        [Route("Send")]
        public async Task<WebResponse<string>> SendBrief([FromBody]Brief brief)
        {
            return await WebResponder.RunWith(async () =>
            {
                var res = await DataQueries.Load<string, dynamic>("SELECT email FROM `brief_users` WHERE name = @name;", new { name = brief.Receiver});
                if (res is null || res?.Count != 1) throw new AppException("Der Empfänger wurde nicht gefunden.");
                var mailAdress = res[0];
                var email = new Email(mailAdress, "Abschlussnachricht: Jemand will dir etwas mitteilen!", brief.Message);
                MailJob.Send(email, "BRIEF");

                return "Deine Nachricht wurde versendet";
                
            }, Request.Path.Value, null, false);
        }

        [HttpGet]
        [Route("Users")]
        public async Task<WebResponse<List<string>>> GetUsers()
        {
            return await WebResponder.RunWith(async () =>
            {
                var res = await DataQueries.Load<string, dynamic>("SELECT name FROM `brief_users` WHERE 1;", new {});
                if (res is null) throw new Exception("Res is null");

                return res;

            }, Request.Path.Value, null, false);
        }

    }

    public record Brief(string Receiver, string Message);
}
