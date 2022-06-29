using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AdminPageController : ControllerBase
    {
        private readonly IMyLogger Logger;
        private readonly IRoutine Routine;
        private readonly IDataQueries DataQueries;
        private readonly IDataAccess DataAccess;

        public AdminPageController(IMyLogger logger, IRoutine routine, IDataQueries dataQueries, IDataAccess dataAccess)
        {
            Logger = logger;
            Routine = routine;
            DataQueries = dataQueries;
            DataAccess = dataAccess;
        }


        [HttpGet]
        [Route("/styles")]
        public IActionResult Styles()
        {
            var buffer = System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Views/styles.css");
            return File(buffer, "text/css");
        }

        [HttpGet]
        [Route("/script")]
        public IActionResult Script()
        {
            var buffer = System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Views/script.js");
            return File(buffer, "text/css");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/Login")]
        public IActionResult LoginView()
        {
            var buffer = System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + @"Views/login.html");
            return File(buffer, "text/html");
        }
        [HttpGet]
        [Route("/Admin")]
        public IActionResult AdminView()
        {
            var buffer = System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + $@"Views/Admin.html");
            return File(buffer, "text/html");
        }

        [HttpGet]
        [Route("/View/{viewName}")]
        public IActionResult View(string viewName)
        {
            try
            {
                var buffer = System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + $@"Views/{viewName}.html");
                return File(buffer, "text/html");
            }
            catch
            {
                return RedirectToAction("AdminView");
            }
        }



        [HttpPost]
        [AllowAnonymous]
        [Route("/login")]
        public async Task<IActionResult> Login()
        {
            var name = Environment.GetEnvironmentVariable("SITE_ADMIN_NAME");
            var pw = Environment.GetEnvironmentVariable("SITE_ADMIN_PW");
            (string name, string pw) admin = (name is null ? string.Empty : name, pw is null ? string.Empty : pw);

            var form = Request.Form;
            if (form["name"] == admin.name && form["pw"] == admin.pw)
            {
                var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, form["pw"]),
                        new Claim(ClaimTypes.Role, "ADMIN")
                    };

                var identity = new ClaimsIdentity(claims, "Admin Identity");
                var principal = new ClaimsPrincipal(new[] { identity });
                var props = new AuthenticationProperties();
                await HttpContext.SignInAsync("cookieAuth", principal, props);
                Console.WriteLine("logged in");
                return RedirectToAction("AdminView");
            }

            return RedirectToAction("LoginView");
        }

        [HttpPost]
        [Route("/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/Login");
        }
    }

}
