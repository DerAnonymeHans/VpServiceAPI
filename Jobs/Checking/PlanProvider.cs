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
using VpServiceAPI.Tools;

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

        public async Task<StatusWrapper<PlanProvideStatus, string>> GetPlanHTML(DateTime date)
        {
            try
            {
                return new(PlanProvideStatus.PLAN_FOUND, await WebScraper.GetFromVP24($"/vplan/vdaten/VplanKl{date.ToString("yyyyMMdd")}.xml?_=1653914187991"));
            }
            catch (Exception ex)
            {
                Logger.Warn(LogArea.PlanProviding, "Failed to get PlanHtml for " + date, ex.Message);
                if (ex.Message.Contains("404")) return new(PlanProvideStatus.PLAN_NOT_FOUND, null);
                return new(PlanProvideStatus.ERROR, null);
            }
        }
    }




    public sealed class ProdPlanProviderKEPLER : IPlanHTMLProvider
    {
        private readonly IMyLogger Logger;
        private readonly IWebScraper WebScraper;


        public ProdPlanProviderKEPLER(IMyLogger logger, IWebScraper webScraper)
        {
            Logger = logger;
            WebScraper = webScraper;
        }

        public async Task<StatusWrapper<PlanProvideStatus, string>> GetPlanHTML(DateTime date)
        {
            var rawHTML = await WebScraper.GetFromKepler("/vertretungsplan");
            int idx = rawHTML.IndexOf($"id='{date:yyyy-MM-dd}'");
            if(idx == -1 ) return new(PlanProvideStatus.PLAN_NOT_FOUND, null);
            var plan = XMLParser.GetNode(rawHTML[idx..], "body");
            if(plan is null) return new(PlanProvideStatus.ERROR, null);
            return new(PlanProvideStatus.PLAN_FOUND, plan);

        }

    }


    



    public sealed class TestPlanProvider : IPlanHTMLProvider
    {
        public async Task<StatusWrapper<PlanProvideStatus, string>> GetPlanHTML(DateTime date)
        {
            string html = File.ReadAllText(@"E:\Projekte\Webseiten\VpService\VpServiceAPI\Jobs\Checking\plan.html");            
            return new(PlanProvideStatus.PLAN_FOUND, html);
        }
    }
}
