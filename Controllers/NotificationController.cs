using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Jobs.Checking;
using VpServiceAPI.Tools;
using VpServiceAPI.Jobs.Notification;
using System.IO;
using VpServiceAPI.WebResponse;
using VpServiceAPI.Exceptions;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Web;
using System.Net.Mime;
using VpServiceAPI.Enums;
using VpServiceAPI.Entities.Notification;


#nullable enable
namespace VpServiceAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class NotificationController : ControllerBase
    {
        private readonly IMyLogger Logger;
        private readonly IRoutine Routine;
        private readonly IDataQueries DataQueries;
        private readonly IArtworkRepository ArtworkRepository;
        private readonly IExtraRepository ExtraRepository;
        private readonly IUserRepository UserRepository;
        private readonly UserTask UserTask;
        private readonly WebResponder WebResponder;



        public NotificationController(IMyLogger logger, IRoutine routine, IDataQueries dataQueries, IArtworkRepository artworkRepository, IUserRepository userRepository, IExtraRepository extraRepository)
        {
            Logger = logger;
            Routine = routine;
            DataQueries = dataQueries;
            ArtworkRepository = artworkRepository;
            UserRepository = userRepository;
            ExtraRepository = extraRepository;
            WebResponder = new(nameof(NotificationController), LogArea.NotificationAPI, false, Logger);
            UserTask = new(Logger, DataQueries, ExtraRepository);
        }


        [HttpGet]
        [Route("Artwork/{artName}/{name}")]
        public async Task<IActionResult> Test(string artName, string name)
        {
            if (!await ArtworkRepository.IncludesArtwork(artName))
            {
                artName = (await ArtworkRepository.Default()).Name;
            }
            var artwork = await ArtworkRepository.GetArtwork(artName);
            using (Bitmap bitMapImage = artwork.GetBitmap())
            {
                Graphics graphicImage = Graphics.FromImage(bitMapImage);
                graphicImage.SmoothingMode = SmoothingMode.AntiAlias;
                graphicImage.DrawString("Hallo", new Font("Arial", 30), new SolidBrush(artwork.FontColor), new Point(50, 80));
                if (name.Length < 12)
                {
                    graphicImage.DrawString(name, new Font("Arial", 48, FontStyle.Bold), new SolidBrush(artwork.FontColor), new Point(50, 120));
                }
                else
                {
                    graphicImage.DrawString(name, new Font("Arial", 40, FontStyle.Bold), new SolidBrush(artwork.FontColor), new Point(50, 120));
                }

                using (var stream = new MemoryStream())
                {
                    bitMapImage.Save(stream, ImageFormat.Png);
                    return File(stream.ToArray(), "image/png");
                }
            }
        }

        [HttpGet]
        [Route("Qrcode")]
        public IActionResult GetQrcode()
        {
            return File(System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Pictures/qrcode.png"), "image/png");
        }
        [HttpGet]
        [Route("Logo.png")]
        public IActionResult GetLogo()
        {
            return File(System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Pictures/logo.png"), "image/png");
        }

        [HttpGet("Badge_VP.png")]
        public IActionResult GetBadge_VP()
        {
            return File(System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Pictures/badge_vp.png"), "image/png");
        }
        [HttpGet("Badge_LS.png")]
        public IActionResult GetBadge_LS()
        {
            return File(System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Pictures/badge_ls.png"), "image/png");
        }

        [HttpGet]
        [Route("GlobalModel")]
        public async Task<WebResponse<GlobalNotificationBody>> GetGlobalModel()
        {
            return await WebResponder.RunWith(async () =>
            {
                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                string json = (await DataQueries.GetRoutineData(RoutineDataSubject.MODEL_CACHE, "global"))[0];
                var body = JsonSerializer.Deserialize<GlobalNotificationBody>(json);
                if (body is null)
                {
                    Logger.Warn(LogArea.NotificationAPI, "Global Notification body is null", user);
                    throw new Exception();
                }
                return body;
            }, Request.Path.Value);
        }

        [HttpGet]
        [Route("GradeModel")]
        public async Task<WebResponse<GradeNotificationBody>> GetGradeModel()
        {
            return await WebResponder.RunWith(async () =>
            {
                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                string json = (await DataQueries.GetRoutineData(RoutineDataSubject.MODEL_CACHE, user.Grade))[0];
                var body = JsonSerializer.Deserialize<GradeNotificationBody>(json);
                if (body is null)
                {
                    Logger.Warn(LogArea.NotificationAPI, "Grade Notification body is null", user);
                    throw new Exception();
                }
                return body;                            
            }, Request.Path.Value);
        }

        [HttpGet]
        [Route("UserModel")]
        public async Task<WebResponse<UserNotificationBody>> GetUserModel()
        {
            return await WebResponder.RunWith(async () =>
            {
                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                var body = (UserNotificationBody?)await UserTask.Begin(user);
                if (body is null)
                {
                    Logger.Warn(LogArea.NotificationAPI, "User Notification body is null", user);
                    throw new Exception();
                }
                return body;
            }, Request.Path.Value);
        }

        [HttpGet("CurrentPlanId")]
        public async Task<WebResponse<string>> GetCurrentPlanId()
        {
            return await WebResponder.RunWith(async () =>
            {
                return (await DataQueries.GetRoutineData(RoutineDataSubject.DATETIME, "plan_found_datetime"))[0];
            }, Request.Path.Value);
        }
        
    }
}
