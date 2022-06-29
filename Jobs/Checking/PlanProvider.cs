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
    public class ProdPlanProviderKEPLER : IPlanHTMLProvider
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


    public class ProdPlanProviderVP24 : IPlanHTMLProvider
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

        public async Task<string> GetPlanHTML(int daysFromToday=0)
        {
            var date = GetDate(daysFromToday);
            try
            {
                PlanHTML = await WebScraper.GetFromVP24($"/vplan/vdaten/VplanKl{date}.xml?_=1653914187991");
            }
            catch(Exception ex)
            {
                Logger.Warn(LogArea.PlanProviding, "Failed to get PlanHtml for " + date, ex.Message);
                PlanHTML = "";
            }
            return PlanHTML;
        }
        private string GetDate(int dayShift=0)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var today = DateTime.Now;
            DateTime date;
            if(today.Hour < 7)
            {
                date = today; 
            }else
            {
                if (today.DayOfWeek == DayOfWeek.Friday)
                {
                    date = today.AddDays(3);
                }
                else if (today.DayOfWeek == DayOfWeek.Saturday)
                {
                    date = today.AddDays(2);
                }
                else
                {
                    date = today.AddDays(1);
                }
            }

            date = date.AddDays(dayShift);
            
            return date.ToString("yyyyMMdd");
        }


    }



    public class TestPlanProvider : IPlanHTMLProvider
    {
        public async Task<string> GetPlanHTML(int daysFromToday)
        {
            string html = File.ReadAllText(@"E:\Projekte\Webseiten\VpService\VpServiceAPI\Jobs\Checking\plan.html");            
            return html;
        }
    }
}
