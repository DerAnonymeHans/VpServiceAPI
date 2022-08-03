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
using System.Collections.Generic;


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
        private readonly IUserRepository UserRepository;
        private readonly WebResponder WebResponder;



        public NotificationController(IMyLogger logger, IRoutine routine, IDataQueries dataQueries, IArtworkRepository artworkRepository, IUserRepository userRepository)
        {
            Logger = logger;
            Routine = routine;
            DataQueries = dataQueries;
            ArtworkRepository = artworkRepository;
            UserRepository = userRepository;
            WebResponder = new();
        }


        [HttpGet]
        [Route("/GetArtwork/{artName}/{name}")]
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
        [Route("/GetQrcode")]
        public IActionResult GetQrcode()
        {
            return File(System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Pictures/qrcode.png"), "image/png");
        }

        [HttpGet]
        [Route("/GetPlanRows")]
        public async Task<WebResponse<List<NotificationRow>>> GetPlanRows()
        {
            return await WebResponder.RunWith(async () =>
            {
                if (Request.Cookies.TryGetValue("user-auth-mail", out string? userAuthMail) && Request.Cookies.TryGetValue("user-auth-hash", out string? userAuthHash))
                {
                    if (userAuthMail is not null && userAuthHash is not null)
                    {
                        if(await UserRepository.IsAuthenticated(userAuthMail, userAuthHash))
                        {
                            var user = await UserRepository.GetUser(userAuthMail);
                            return await DataQueries.Load<NotificationRow, dynamic>("SELECT * FROM notification_rows WHERE klasse LIKE '%@grade%'", new { grade = user.Grade });
                        }
                    }

                }
                throw new AppException("Du musst angemeldet sein um die Plandaten zu sehen.");
            }, Request.Path.Value);
        }
        
    }
}
