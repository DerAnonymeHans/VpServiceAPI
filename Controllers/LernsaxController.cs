using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Lernsax;
using VpServiceAPI.Entities.Persons;
using VpServiceAPI.Enums;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Interfaces.Lernsax;
using VpServiceAPI.WebResponse;

namespace VpServiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LernsaxController : ControllerBase
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private readonly IUserRepository UserRepository;
        private readonly ILernsaxMailService EmailService;
        private readonly WebResponder WebResponder;

        public LernsaxController(IMyLogger logger, IDataQueries dataQueries, IUserRepository userRepository, ILernsaxMailService emailGetter)
        {
            Logger = logger;
            DataQueries = dataQueries;
            UserRepository = userRepository;
            WebResponder = new("Lernsax", LogArea.LernsaxAPI, false, logger);
            EmailService = emailGetter;
        }

        [HttpPost]
        [Route("SetCredentials")]
        public async Task<WebMessage> SetCredentials()
        {
            return await WebResponder.RunWith(async () =>
            {
                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                Request.Form.TryGetValue("mail", out StringValues mailValues);
                Request.Form.TryGetValue("password", out StringValues passwordValues);
                if (mailValues.Count != 1) throw new AppException("Du musst deine Lernsax Emailadresse angeben.");
                if (passwordValues.Count != 1) throw new AppException("Du musst dein Lernsax Passwort angeben.");
                string mail = mailValues[0]; 
                string password = passwordValues[0];

                var credentials = new LernsaxCredentials(mail, password);
                var lernsax = new Lernsax(Array.Empty<LernsaxService>(), credentials);
                var userWithLernsax = new UserWithLernsax(user, lernsax);
                await UserRepository.Lernsax.SetLernsax(userWithLernsax, new[] { LernsaxData.CREDENTIALS });

                await UserRepository.Lernsax.Login(user, null);
            }, Request.Path.Value, "Deine Anmeldedaten wurden gespeichert.");
        }
        [HttpGet("HasValidCredentials")]
        public async Task<WebResponse<bool>> HasValidCredentials()
        {
            return await WebResponder.RunWith(async () =>
            {
                try
                {
                    var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                    await UserRepository.Lernsax.Login(user, null);
                    return true;
                }
                catch
                {
                    return false;
                }
            }, Request.Path.Value);
        }

        [HttpDelete("Credentials")]
        public async Task<WebMessage> DeleteCredentials()
        {
            return await WebResponder.RunWith(async () =>
            {
                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                await DataQueries.Save("UPDATE lernsax SET credentials='' WHERE userId=@userId", new { userId = user.Id });

            }, Request.Path.Value, "Deine Anmeldedaten wurden gelöscht.");
        }        

        // UN_SUBSCRIBE SERVICE
        [HttpPost("Services/{method}/{service}")]
        public async Task<WebMessage> Un_SubscribeService(string method, LernsaxService service)
        {
            if(method != "Subscribe" && method != "Unsubscribe")
            {
                Response.StatusCode = 400;
                return new WebMessage
                {
                    IsSuccess = false,
                    Message = $"Method '{method}' not supported. Only use Subscribe or Unsubscribe."
                };
            }

            return await WebResponder.RunWith(async () =>
            {
                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                var services = await UserRepository.Lernsax.GetServices(user);
                var newServices = services.ToList();
                if(method == "Subscribe")
                {
                    if (services.Contains(service)) return;
                    newServices.Add(service);
                    _ = service switch
                    {
                        LernsaxService.MAIL => EmailService.RunOnUser(user),
                        _ => Task.CompletedTask
                    };
                }
                else newServices.Remove(service);

                await UserRepository.Lernsax.SetLernsax(user, new Lernsax(newServices.ToArray()), new[] { LernsaxData.SERVICES });
            }, Request.Path.Value, $"Du hast den Service erfolgreich {(method == "Subscribe" ? "abonniert" : "deabonniert")}.");
        }

        [HttpGet("SubscribedServices")]
        public async Task<WebResponse<LernsaxService[]>> GetSubscribedServices()
        {
            return await WebResponder.RunWith(async () =>
            {
                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                return await UserRepository.Lernsax.GetServices(user);
            }, Request.Path.Value);
        }

        [HttpDelete("All")]
        public async Task<WebMessage> DeleteAll()
        {
            return await WebResponder.RunWith(async () =>
            {
                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                await UserRepository.Lernsax.SetLernsax(user, new Lernsax(Array.Empty<LernsaxService>()), new[] { LernsaxData.ALL });
            }, Request.Path.Value, "Deine Lernsaxdaten wurden gelöscht.");
        }


        [HttpGet("Service/Mail/Heads")]
        public async Task<WebResponse<List<LernsaxMailHead>>> GetMailHeads()
        {
            return await WebResponder.RunWith(async () =>
            {
                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                var mails = await EmailService.GetStoredEmails(user);
                if (mails is null) return new List<LernsaxMailHead>();
                return mails.Select(mail => new LernsaxMailHead { Sender=mail.Sender, DateTime=mail.DateTime, Subject=mail.Subject}).ToList();
            }, Request.Path.Value);
        }
        [HttpGet("Service/Mail/MailBody/{mailId}")]
        public async Task<WebResponse<string>> GetMail(string mailId)
        {
            return await WebResponder.RunWith(async () =>
            {
                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                var mails = await EmailService.GetStoredEmails(user);
                if (mails is null) throw new AppException("Es wurden keine Emails gefunden.");
                var selectedMail = mails.Find(mail => mail.Id.TrimEnd() == mailId);
                if (selectedMail is null) throw new AppException("Die Email wurde nicht gefunden.");
                return selectedMail.Body;
            }, Request.Path.Value);
        }
        
        [HttpPost("Service/Mail/SearchBody")]
        public async Task<WebResponse<List<string>>> SearchMailBody() // returns list of mail ids
        {
            return await WebResponder.RunWith(async () =>
            {
                var searchString = Request.Form["search"][0];
                var user = await UserRepository.GetAuthenticatedUserFromRequest(Request.Cookies);
                var mails = await EmailService.GetStoredEmails(user);
                if (mails is null) throw new AppException("Es wurden keine Emails gefunden.");

                var ids = new List<string>();
                foreach(var mail in mails)
                {
                    if (!mail.Body.ToLower().Contains(searchString)) continue;
                    ids.Add(mail.Id);
                }
                return ids;
            }, Request.Path.Value);
        }
    }
}
