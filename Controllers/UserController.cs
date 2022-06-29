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


#nullable enable
namespace VpServiceAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
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
        [Route("/GetUserCount")]
        public async Task<WebResponse<int>> GetUserCount()
        {
            return await WebResponder.RunWith(async () => (await DataQueries.Load<int, dynamic>("SELECT COUNT(id) AS count FROM users WHERE 1", new { }))[0], Request.Path.Value, null, false);
        }
        [HttpPost]
        [Route("/Subscribe")]
        public async Task<WebMessage> Subscribe()
        {
            return await WebResponder.RunWith(async () =>
            {
                var form = Request.Form;
                if (form["accept-agb"] != "on") throw new AppException("Bitte akzeptiere zuerst die AGB.");
                var user = await UserRepository.ValidateUser(form["name"], form["mail"], form["grade"]);
                await UserRepository.AddUserRequest(user);
            }, Request.Path.Value, "Es hat geklappt! Jetzt muss die Anfrage nur noch manuell geprüft werden, dann erhälst auch du die Mails.");
        }
        [HttpPost]
        [Route("/ProposeSmallExtra")]
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
        [Route("/Proposal")]
        public async Task<WebMessage> Propose()
        {
            return await WebResponder.RunWith(async () =>
            {
                if (string.IsNullOrWhiteSpace(Request.Form["text"])) throw new AppException("Das Textfeld darf nicht leer sein");
                if (string.IsNullOrEmpty(Request.Form["text"])) throw new AppException("Das Textfeld darf nicht leer sein");
                AttackDetector.Detect(Request.Form["text"]);
                await DataQueries.Save("INSERT INTO proposals(text) VALUES (@text)", new { text = Request.Form["text"] });
            }, Request.Path.Value, "Es hat geklappt! Deine Anregung wird in kürze beachtet werden.");            
        }
    }
}
