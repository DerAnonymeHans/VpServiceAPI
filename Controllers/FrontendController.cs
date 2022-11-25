using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using VpServiceAPI.Tools;

namespace VpServiceAPI.Controllers
{
    [Route("[controller]")]
    public sealed class FrontendController : ControllerBase
    {
        [HttpGet]
        [Route("/")]
        [Route("/Benachrichtigung")]
        [Route("/Statistiken")]
        [Route("/Mitmachen")]
        public IActionResult Index()
        {
            var buffer = System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Frontend/index.html");
            return File(buffer, "text/html");
        }

        [HttpGet("/{fileName}")]
        public IActionResult Root(string fileName)
        {
            if (!fileName.Contains(".")) return NotFound();
            try
            {
                var buffer = System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @$"Frontend/{fileName}");
                return File(buffer, GetContentType(fileName.Split('.').Last()));
            }catch
            {
                return NotFound();
            }
        }

        [HttpGet("/assets/{fileName}")]
        public IActionResult Assets(string fileName)
        {
            if (!fileName.Contains(".")) return NotFound();
            try
            {
                var buffer = System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @$"Frontend/assets/{fileName}");
                return File(buffer, GetContentType(fileName.Split('.').Last()));
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpGet("/img/icons/{fileName}")]
        public IActionResult Icons(string fileName)
        {
            if (!fileName.Contains(".")) return NotFound();
            try
            {
                var buffer = System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @$"Frontend/assets/img/icons{fileName}");
                return File(buffer, GetContentType(fileName.Split('.').Last()));
            }
            catch
            {
                return NotFound();
            }
        }

        private string GetContentType(string extension)
        {
            return extension switch
            {
                "js" => "application/javascript",
                "json" => "application/json",
                "ico" => "image/x-icon",
                "png" => "image/png",
                "jpg" => "image/jpg",
                "css" => "text/css",
                _ => "plain/text"
            };
        }
    }
}
