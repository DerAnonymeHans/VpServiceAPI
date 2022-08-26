﻿using System;
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
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Web;


#nullable enable
namespace VpServiceAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
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
        [Route("GetArtwork/{artName}/{name}")]
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
        [Route("GetQrcode")]
        public IActionResult GetQrcode()
        {
            return File(System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Pictures/qrcode.png"), "image/png");
        }
        [HttpGet]
        [Route("GetLogo")]
        public IActionResult GetLogo()
        {
            return File(System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Pictures/logo.png"), "image/png");
        }
        [HttpGet]
        [Route("GlobalModel")]
        public async Task<WebResponse<GlobalNotificationBody>> GetGlobalModel()
        {
            return await WebResponder.RunWith(async () =>
            {
                var user = await UserRepository.TryGetAuthenticatedUserFromRequest(Request.Cookies);
                string json = (await DataQueries.GetRoutineData("MODEL_CACHE", "global"))[0];
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
                var user = await UserRepository.TryGetAuthenticatedUserFromRequest(Request.Cookies);
                string json = (await DataQueries.GetRoutineData("MODEL_CACHE", user.Grade))[0];
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
                var user = await UserRepository.TryGetAuthenticatedUserFromRequest(Request.Cookies);
                var body = (UserNotificationBody?)await UserTask.Begin(user);
                if (body is null)
                {
                    Logger.Warn(LogArea.NotificationAPI, "User Notification body is null", user);
                    throw new Exception();
                }
                return body;
            }, Request.Path.Value);
        }

        [HttpGet]
        [Route("IsNewPlan/{originDate}")]
        public async Task<WebResponse<bool>> IsNewPlan(string? originDate)
        {
            return await WebResponder.RunWith(async () =>
            {
                if (string.IsNullOrWhiteSpace(originDate)) return true;
                var newOriginDate = (await DataQueries.GetRoutineData("DATETIME", "last_origin_datetime"))[0];
                return newOriginDate != originDate;
            }, Request.Path.Value);
        }
        
    }
}
