using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using VpServiceAPI.Interfaces;

#nullable enable
namespace VpServiceAPI.Jobs.Checking
{
    public sealed class ProdPlanProviderKEPLER : IPlanHTMLProvider
    {
        private readonly IMyLogger Logger;
        private readonly IWebScraper WebScraper;

        private string Url { get; } = "https://jkg-leipzig.de";
        public string PlanHTML { get; set; } = "";

        public ProdPlanProviderKEPLER(IMyLogger logger, IWebScraper webScraper)
        {
            Logger = logger;
            WebScraper = webScraper;
        }

        public async Task<string> GetPlanHTML(int daysFromToday)
        {
            if (daysFromToday > 0) return "";
            //PlanHTML = await GetRawPlanHTML();
            PlanHTML = await WebScraper.GetFromKepler("/vertretungsplan");
            PlanHTML = MinimizePlan(PlanHTML);
            return PlanHTML;
        }

        private string MinimizePlan(string rawPlan)
        {
            try
            {
                int planStart = rawPlan.IndexOf("<title>Vertretungsplan</title>");
                return rawPlan[planStart..];
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.PlanProviding, ex, "Tried to minimize plan.", rawPlan);
                return rawPlan;
            }

        }
    }


    public sealed class ProdPlanProviderVP24 : IPlanHTMLProvider
    {
        private readonly IMyLogger Logger;
        private readonly IWebScraper WebScraper;

        private string Url { get; } = "https://www.stundenplan24.de/10073128/";
        public string PlanHTML { get; set; } = "";

        public ProdPlanProviderVP24(IMyLogger logger, IWebScraper webScraper)
        {
            Logger = logger;
            WebScraper = webScraper;
        }

        public async Task<string> GetPlanHTML(int dayShift=0)
        {
            var date = GetDate(dayShift);
            try
            {
                PlanHTML = await WebScraper.GetFromVP24($"/vplan/vdaten/VplanKl{date}.xml?_=1653914187991");
            }
            catch(Exception ex)
            {
                Logger.Info(LogArea.PlanProviding, "Failed to get PlanHtml for " + date, ex.Message);
                PlanHTML = "";
            }
            return PlanHTML;
        }
        private string GetDate(int dayShift=0)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");           

            Func<DateTime> GetWeekday = () =>
            {
                var today = DateTime.Now;
                // skip weekends
                if (today.DayOfWeek == DayOfWeek.Saturday)
                {
                    return today.AddDays(2);
                }
                if (today.DayOfWeek == DayOfWeek.Sunday)
                {
                    return today.AddDays(1);
                }

                // plan for next weekday only after 7 o clock
                if (today.Hour < 7)
                {
                    return today;
                }
                // next weekday after friday is monday
                if (today.DayOfWeek == DayOfWeek.Friday)
                {
                    return today.AddDays(3);
                }
                return today.AddDays(1);
            };

            DateTime date = GetWeekday().AddDays(dayShift);
            
            return date.ToString("yyyyMMdd");
        }


    }



    public sealed class TestPlanProvider : IPlanHTMLProvider
    {
        public async Task<string> GetPlanHTML(int daysFromToday)
        {
            string html = File.ReadAllText(@"E:\Projekte\Webseiten\VpService\VpServiceAPI\Jobs\Checking\plan.html");            
            return html;
        }
    }
}
