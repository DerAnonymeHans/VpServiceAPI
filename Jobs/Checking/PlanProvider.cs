using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Tools;
using VpServiceAPI.Enums;
using VpServiceAPI.Interfaces;

#nullable enable
namespace VpServiceAPI.Jobs.Checking
{

    public sealed class ProdPlanProviderVP24 : IPlanHTMLProvider
    {
        private readonly IMyLogger Logger;
        private readonly IWebScraper WebScraper;

        public ProdPlanProviderVP24(IMyLogger logger, IWebScraper webScraper)
        {
            Logger = logger;
            WebScraper = webScraper;
        }

        public async Task<StatusWrapper<PlanProvideStatus, string>> GetPlanHTML(int dayShift = 0)
        {
            var date = GetDate(dayShift);
            try
            {
                return new(PlanProvideStatus.PLAN_FOUND, await WebScraper.GetFromVP24($"/vplan/vdaten/VplanKl{date}.xml?_=1653914187991"));
            }
            catch (Exception ex)
            {
                Logger.Warn(LogArea.PlanProviding, "Failed to get PlanHtml for " + date, ex.Message);
                if (ex.Message.Contains("404")) return new(PlanProvideStatus.PLAN_NOT_FOUND, null);
                return new(PlanProvideStatus.ERROR, null);
            }
        }

        // plan for next day after 7 o clock
        // jumps over weekends
        private string GetDate(int dayShift = 0)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            bool isWeekend(DateTime day) => day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday;

            var date = DateTime.Now;
            date = date.AddDays(date.Hour < 7 ? 0 : 1);
            date = date.AddDays(-1);
            for (int i = -1; i < dayShift; i++)
            {
                date = date.AddDays(1);
                if (!isWeekend(date)) continue;
                date = date.AddDays(date.DayOfWeek == DayOfWeek.Saturday ? 2 : 1);

            }
            return date.ToString("yyyyMMdd");
        }


    }




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

        public async Task<StatusWrapper<PlanProvideStatus, string>> GetPlanHTML(int daysFromToday)
        {
            if (daysFromToday > 0) return new(PlanProvideStatus.PLAN_NOT_FOUND, null);
            //PlanHTML = await GetRawPlanHTML();
            PlanHTML = await WebScraper.GetFromKepler("/vertretungsplan");
            PlanHTML = MinimizePlan(PlanHTML);
            return new(PlanProvideStatus.PLAN_FOUND, PlanHTML);
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


    



    public sealed class TestPlanProvider : IPlanHTMLProvider
    {
        public async Task<StatusWrapper<PlanProvideStatus, string>> GetPlanHTML(int daysFromToday)
        {
            string html = File.ReadAllText(@"E:\Projekte\Webseiten\VpService\VpServiceAPI\Jobs\Checking\plan.html");            
            return new(PlanProvideStatus.PLAN_FOUND, html);
        }
    }
}
