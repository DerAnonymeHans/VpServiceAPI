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


#nullable enable
namespace VpServiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class UserController : ControllerBase
    {
        private readonly IMyLogger Logger;
        private readonly IRoutine Routine;
        private readonly IDataQueries DataQueries;
        private readonly IArtworkRepository ArtworkRepository;
        private readonly IUserRepository UserRepository;
        private readonly IExtraRepository ExtraRepository;

        private readonly WebResponder WebResponder;

        public UserController(IMyLogger logger, IRoutine routine, IDataQueries dataQueries, IArtworkRepository artworkRepository, IUserRepository userRepository, IExtraRepository extraRepository)
        {
            Logger = logger;
            Routine = routine;
            DataQueries = dataQueries;
            ArtworkRepository = artworkRepository;
            UserRepository = userRepository;
            ExtraRepository = extraRepository;

            WebResponder = new("User", LogArea.UserAPI, false, logger);
        }


        [HttpGet]
        [Route("UserCount")]
        public async Task<WebResponse<int>> GetUserCount()
        {
            return await WebResponder.RunWith(async () => (await DataQueries.Load<int, dynamic>("SELECT COUNT(id) AS count FROM users WHERE 1", new { }))[0], Request.Path.Value, null, false);
        }
        [HttpPost]
        [Route("Subscribe")]
        public async Task<WebMessage> Subscribe()
        {
            var res = await WebResponder.RunWith(async () =>
            {
                var form = Request.Form;
                if (form["accept-agb"] != "on") throw new AppException("Bitte akzeptiere zuerst die AGB.");
                var user = await UserRepository.ValidateUser(form["name"], form["mail"], form["grade"], form["notify-mode"]);
                await UserRepository.AddUserRequest(user);
            }, Request.Path.Value, $"Es hat geklappt! Jetzt muss deine Anfrage nur noch manuell geprüft werden, dies kann bis zu ein paar Tage dauern.");
            return res;
        }
        [HttpPost]
        [Route("ProposeSmallExtra")]
        public async Task<WebMessage> ProposeSmallExtra()
        {
            return await WebResponder.RunWith(async () =>
            {
                var form = Request.Form;
                await ExtraRepository.AddSmallExtraProposal(new SmallExtra
                {
                    Text = form["text"],
                    Author = form["author"]
                });
            }, Request.Path.Value, "Es hat geklappt! Jetzt muss die Anfrage nur noch manuell geprüft werden, dann kann das kleine Extra in Emails erscheinen.");
        }

        [HttpPost]
        [Route("Proposal")]
        public async Task<WebMessage> Propose()
        {
            return await WebResponder.RunWith(async () =>
            {
                if (string.IsNullOrWhiteSpace(Request.Form["text"])) throw new AppException("Das Textfeld darf nicht leer sein");
                AttackDetector.Detect(Request.Form["text"]);
                await DataQueries.Save("INSERT INTO proposals(text) VALUES (@text)", new { text = Request.Form["text"] });
            }, Request.Path.Value, "Es hat geklappt! Deine Anregung wird in kürze beachtet werden.");            
        }

        [HttpGet]
        [Route("IsAuthenticated")]
        public async Task<WebResponse<bool>> IsUserAuthenticated()
        {
            if(Request.Cookies.TryGetValue("userAuthMail", out string? userAuthMail) && Request.Cookies.TryGetValue("userAuthHash", out string? userAuthHash))
            {
                if(userAuthMail is not null && userAuthHash is not null)
                {
                    return await WebResponder.RunWith(async () => await UserRepository.IsAuthenticated(userAuthMail, userAuthHash), Request.Path.Value);
                }
                
            }
            return new WebResponse<bool>
            {
                Body = false,
                IsSuccess = true,
                Message = "Cookies not provided"
            };
        }

        [HttpPost]
        [Route("RequestHashReset")]
        public async Task<WebMessage> RequestHashReset()
        {
            Request.Form.TryGetValue("mail", out StringValues mail);            
            return await WebResponder.RunWith(async () => await UserRepository.SendHashResetMail(mail[0]), Request.Path.Value, "Die Email wurde versendet.");
        }

        [HttpPost]
        [Route("ResetHash")]
        public async Task<WebMessage> SetAuthentication()
        {
            return await WebResponder.RunWith(async () =>
            {
                //Request.Form.TryGetValue("mail", out StringValues mail);
                Request.Form.TryGetValue("key", out StringValues key);
                var pair = await UserRepository.EndHashResetAndGetMailHashPair(key);

                DateTime expireDate = DateTime.UtcNow;
                expireDate = expireDate.AddYears(10);
                expireDate = DateTime.SpecifyKind(expireDate, DateTimeKind.Utc);
                DateTimeOffset utcTime2 = expireDate;

                Response.Cookies.Append("userAuthMail", pair.Mail, new CookieOptions
                {
                    SameSite = SameSiteMode.None,
                    Secure = true,
                    Expires = expireDate
                });
                Response.Cookies.Append("userAuthHash", pair.Hash, new CookieOptions
                {
                    SameSite = SameSiteMode.None,
                    Secure = true,
                    Expires = expireDate
                });

            }, Request.Path.Value, "Du wurdest erfolgreich angemeldet.");
        }

        [HttpPost]
        [Route("ChangeNotifyMode/{mode}")]
        public async Task<WebMessage> ChangeNotifyMode(string mode)
        {
            return await WebResponder.RunWith(async () =>
            {
                var _mode = mode switch
                {
                    "pwa" => NotifyMode.PWA,
                    "mail" => NotifyMode.EMAIL,
                    _ => throw new AppException("Bitte gib einen validen Benachrichtigungsweg an.")
                };

                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);

                await DataQueries.Save("UPDATE users SET mode=@mode WHERE address=@mail", new { mode = _mode.ToString(), mail = user.Address });

            }, Request.Path.Value, "Der Benachrichtigunsweg wurde geändert.");
        }
        [HttpGet]
        [Route("GetNotifyMode")]
        public async Task<WebResponse<string>> GetNotifyMode()
        {
            return await WebResponder.RunWith(async () =>
            {
                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                return user.NotifyMode.ToString();
            }, Request.Path.Value);
        }


        [HttpPost]
        [Route("SetPushSubscribtion")]
        public async Task<WebMessage> SetPushSubscribtion()
        {
            return await WebResponder.RunWith(async () =>
            {
                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                string subscribtion = Request.Form["subscribtion"];
                await DataQueries.Save("UPDATE users SET push_subscribtion=@subscribtion WHERE address=@mail", new { subscribtion, mail = user.Address });

            }, Request.Path.Value, "Push_subscribtion wurde gesetzt.");
        }

        [HttpGet]
        [Route("GetPushSubscribtion")]
        public async Task<WebResponse<string>> GetPushSubscribtion()
        {
            return await WebResponder.RunWith(async () =>
            {
                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                return (await DataQueries.Load<string, dynamic>("SELECT push_subscribtion FROM users WHERE address=@mail", new { mail = user.Address }))[0];
            }, Request.Path.Value, "Push_subscribtion wurde gesetzt.");
        }

        [HttpPost]
        [Route("ConfirmPush")]
        public async void ConfirmPush()
        {
            Logger.Info(LogArea.UserAPI, "Confirming Push to: " + await new StreamReader(Request.Body).ReadToEndAsync());
        }

    }
}
