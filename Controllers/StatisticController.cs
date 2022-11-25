using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using VpServiceAPI.Entities.Statistics;
using VpServiceAPI.Enums;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Jobs.StatProviding;
using VpServiceAPI.Tools;
using VpServiceAPI.WebResponse;

namespace VpServiceAPI.Controllers
{
    [Route("api/Statistic")]
    public sealed class StatisticController : ControllerBase
    {
        private readonly IMyLogger Logger;
        private readonly IByGeneralProvider GeneralProvider;
        private readonly IByCountProvider CountProvider;
        private readonly IByTimeProvider TimeProvier;
        private readonly IByWhoProvider WhoProvider;
        private readonly IByComparisonProvider ComparisonProvider;
        private readonly IByMetaProvider MetaProvider;

        private readonly WebResponder WebResponder;

        public StatisticController(IMyLogger logger, IByGeneralProvider generalProvider, IByCountProvider countProvider, IByTimeProvider timeProvider, IByWhoProvider whoProvider, IByComparisonProvider comparisonProvider, IByMetaProvider metaProvider)
        {
            Logger = logger;
            GeneralProvider = generalProvider;
            CountProvider = countProvider;
            TimeProvier = timeProvider;
            WhoProvider = whoProvider;
            ComparisonProvider = comparisonProvider;
            MetaProvider = metaProvider;

            WebResponder = new("Statistic", LogArea.StatAPI, false, logger);
        }

        [HttpPost]
        [Route("Login")]
        public WebMessage Login()
        {
            try
            {
                var form = Request.Form;
                bool isNormalAuth = form["name"].ToString().ToLower() == Environment.GetEnvironmentVariable("SITE_STATS_NAME") && form["pw"] == Environment.GetEnvironmentVariable("SITE_STATS_PW");
                bool isSpecialAuth = form["name"] == "relaxdays" && form["pw"] == "re!axdays01";

                if (!isNormalAuth && !isSpecialAuth)
                    throw new AppException("Nutzername oder Passwort sind nicht korrekt.");

                if (isSpecialAuth) Logger.Info("Relaxdays has opened the website");

                if (form["accept-agb"] != "on") throw new AppException("Bitte akzeptieren Sie zuerst die AGB");

                Response.Cookies.Append("statAuth", Environment.GetEnvironmentVariable("SITE_STAT_AUTH") ?? "", new CookieOptions
                {
                    SameSite = SameSiteMode.None,
                    Secure = true
                });
                return new WebMessage
                {
                    IsSuccess = true,
                    Message = "Sie haben sich erfolgreich angemeldet und können sich nun die Statistiken anschauen."
                };
            }
            catch(AppException ex)
            {
                return new WebMessage
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }catch(Exception ex)
            {
                Logger.Error(LogArea.StatAPI, ex, "Tried to login to stats");
                return new WebMessage
                {
                    IsSuccess = false,
                    Message = "Leider ist etwas schief gelaufen. Bitte versuche es später nochmal."
                };
            }            
        }
        [HttpPost]
        [Route("Logout")]
        public WebMessage Logout()
        {
            Response.Cookies.Delete("statAuth", new CookieOptions
            {
                SameSite = SameSiteMode.None,
                Secure = true
            });
            return new WebMessage
            {
                IsSuccess = true,
                Message = "Sie wurden erfolgreich abgemeldet."
            };
        }
        [HttpGet]
        [Route("IsLoggedIn")]
        public bool IsLoggedIn() // only accessible when passing middleware, so response always true (method is just a test if user is logged in, any other action of the controller would also work)
        {
            return true;
        }

        [HttpGet]
        [Route("CheckDataFreshness")]
        public async Task<WebMessage> CheckDataFreshness() 
        {
            return await WebResponder.RunWith(async () => await GeneralProvider.CheckDataFreshness(), Request.Path.Value);
        }
        [HttpGet]
        [Route("CheckDataAmount")]
        public async Task<WebMessage> CheckDataAmount()
        {
            return await WebResponder.RunWith(async () => await GeneralProvider.CheckDataAmount(), Request.Path.Value);
        }

        [HttpGet]
        [Route("Names/{entityType}")]
        public async Task<WebResponse<List<string>>> GetNames(EntityType entityType)
        {
            return await WebResponder.RunWith(async () => await GeneralProvider.GetNames(entityType), Request.Path.Value);
        }
        [HttpGet]
        [Route("RecordedDays/Count")]
        public async Task<WebResponse<int>> RecordedDaysCount()
        {
            return await WebResponder.RunWith(async () => await GeneralProvider.GetDaysCount(), Request.Path.Value);
        }
        [HttpGet]
        [Route("Years")]
        public async Task<WebResponse<List<string>>> GetYears()
        {
            return await WebResponder.RunWith(async () => await GeneralProvider.GetYears(), Request.Path.Value);
        }
        [HttpGet]
        [Route("Years/Current")]
        public WebResponse<string> GetCurrentYear()
        {
            return WebResponder.RunWithSync(() => ProviderHelper.CurrentSchoolYear, Request.Path.Value);
        }

        [HttpGet]
        [Route("CountOf/{name}")]
        public async Task<WebResponse<CountStatistic>> CountOf(string name)
        {
            name = WebUtility.UrlDecode(name);
            return await WebResponder.RunWith(async () => await CountProvider.GetCountOf(name), Request.Path.Value);
        }
        [HttpGet]
        [Route("SortCountsOf/{includeWho}/By/{sortBy}")]
        public async Task<WebResponse<List<CountStatistic>>> SortCountsOf(EntityType includeWho, string sortBy)
        {
            return await WebResponder.RunWith(async () => await CountProvider.GetSortedCountsBy(includeWho, sortBy), Request.Path.Value);            
        }


        [HttpGet]
        [Route("CountsOf/{name}/Over/{timeType}")]
        public async Task<WebResponse<TimeStatistic>> CountOfOver(string name, TimeType timeType)
        {
            name = WebUtility.UrlDecode(name);
            return await WebResponder.RunWith(async () => await TimeProvier.GetTimesOf(timeType, name), Request.Path.Value);
        }
        [HttpGet]
        [Route("SortCountsOf/{includeWho}/During/{timeName}/By/{sortBy}")]
        public async Task<WebResponse<List<CountStatistic>>> SortCountsOf(EntityType includeWho, string timeName, string sortBy)
        {
            return await WebResponder.RunWith(async () => 
            {
                var timeType = Converter.TimeNameToType(timeName);
                if (timeType == null) throw new AppException("Die Zeitangabe wurde nicht gefunden.");
                return await TimeProvier.GetSortedTimeBy((TimeType)timeType, timeName, includeWho, sortBy);
            }, Request.Path.Value);
        }


        [HttpGet]
        [Route("Compare/{name}/WithName/{otherName}")]
        public async Task<WebResponse<WhoStatistic>> CompareWithName(string name, string otherName)
        {
            name = WebUtility.UrlDecode(name);
            return await WebResponder.RunWith(async () => await WhoProvider.GetRelationToSpecific(name, otherName), Request.Path.Value);
        }
        [HttpGet]
        [Route("Compare/{name}/WithType/{entityType}")]
        public async Task<WebResponse<List<WhoStatistic>>> CompareWithType(string name, EntityType entityType)
        {
            name = WebUtility.UrlDecode(name);
            return await WebResponder.RunWith(async () => await WhoProvider.GetRelationToType(name, entityType), Request.Path.Value);
        }
        [HttpGet]
        [Route("SortRelations/By/{sortBy}")]
        public async Task<WebResponse<List<WhoStatistic>>> SortRelations(string sortBy)
        {
            return await WebResponder.RunWith(async () => await WhoProvider.SortRelations(sortBy), Request.Path.Value);
        }

        [HttpGet]
        [Route("RelativeOf/{name}")]
        public async Task<WebResponse<RelativeStatistic>> RelativeOf(string name)
        {
            name = WebUtility.UrlDecode(name);
            return await WebResponder.RunWith(async () => await ComparisonProvider.RelativeOf(name), Request.Path.Value);
        }
        [HttpGet]
        [Route("RelativeOfType/{entityType}")]
        public async Task<WebResponse<RelativeStatistic>> RelativeOfType(EntityType entityType)
        {
            return await WebResponder.RunWith(async () => await ComparisonProvider.RelativeOfType(entityType), Request.Path.Value);
        }
        [HttpGet]
        [Route("SortRelativesOf/{includeWho}/By/{sortBy}")]
        public async Task<WebResponse<List<RelativeStatistic>>> SortRelativesOf(EntityType includeWho, string sortBy)
        {
            return await WebResponder.RunWith(async () => await ComparisonProvider.SortRelativeOf(includeWho, sortBy), Request.Path.Value);
        }
        [HttpGet]
        [Route("RelativeOf/{name}/InComparison")]
        public async Task<WebResponse<ComparisonStatistic>> RelativeOfInComparison(string name)
        {
            name = WebUtility.UrlDecode(name);
            return await WebResponder.RunWith(async () => await ComparisonProvider.RelativeOfInComparison(name), Request.Path.Value);
        }
        [HttpGet]
        [Route("AverageOf/{entityType}")]
        public async Task<WebResponse<CountStatistic>> AverageOf(EntityType entityType)
        {
            return await WebResponder.RunWith(async () => await ComparisonProvider.AverageOf(entityType), Request.Path.Value);
        }

        [HttpGet]
        [Route("SortCountsOf/Extras/{sortBy}")]
        public async Task<WebResponse<List<KeyCountStatistic>>> SortCountsOfExtras(string sortBy)
        {
            return await WebResponder.RunWith(async () => await MetaProvider.SortCountsOfExtras(sortBy), Request.Path.Value);
        }
    }

}
