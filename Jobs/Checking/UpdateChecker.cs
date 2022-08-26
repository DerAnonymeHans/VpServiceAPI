using System;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.Checking
{
    public class UpdateChecker : IUpdateChecker
    {
        private readonly IPlanHTMLProvider PlanHTMLProvider;
        private readonly IPlanConverter PlanConverter;
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        public PlanModel PlanModel { get; set; } = new();

        public UpdateChecker(IMyLogger logger, IPlanHTMLProvider planHTMLProvider, IDataQueries dataQueries, IPlanConverter planConverter)
        {
            Logger = logger;
            PlanHTMLProvider = planHTMLProvider;
            DataQueries = dataQueries;
            PlanConverter = planConverter;
        }

        public async Task<bool?> Check(bool isSecondPlan=false, int dayShift=0)
        {            
            string html = await PlanHTMLProvider.GetPlanHTML(isSecondPlan ? 1 + dayShift : 0 + dayShift);
            if (string.IsNullOrEmpty(html)) return null;
            var testModel = PlanConverter.Convert(html);
            if(testModel == null) return null;
            PlanModel = testModel;

            if (isSecondPlan) return true;


            if(await IsForceMail())
            {
                Logger.Debug("Force");
                PlanModel._forceNotify = true;
                return true;
            }

            if (!await IsPlanNew()) return false;

            return true;
        }
        private async Task<bool> IsForceMail()
        {
            bool isForceOnInfoChange = false;
            try
            {
                isForceOnInfoChange = (await DataQueries.GetRoutineData("FORCE_MODE", "on_info_change"))[0] == "true";
            }catch(Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to determine if mail should be force to send");
            }

            if (isForceOnInfoChange)
            {
                if (await IsInfoChange()) return true;
            }
            return false;
        }
        private async Task<bool> IsInfoChange()
        {
            try
            {
                var cached = (await DataQueries.GetRoutineData("CACHE", "information"))[0];
                if (cached != string.Join('|', PlanModel.Information))
                {
                    await DataQueries.SetRoutineData("CACHE", "information", string.Join('|', PlanModel.Information));
                    Logger.Info(LogArea.Notification, "Forcing new Mail because of new information");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to compare cached 'information' with planmodel informaion.", PlanModel.Information);
            }
            return false;
        }
        private async Task<bool> IsPlanNew()
        {
            if (Environment.GetEnvironmentVariable("MODE") == "Testing") return true;
            try
            {
                var lastPlanTime = (await DataQueries.GetRoutineData("DATETIME", "last_origin_datetime"))[0];
                if(lastPlanTime != PlanModel.MetaData.OriginDate.DateTime && Environment.GetEnvironmentVariable("VP_SOURCE") != "STATIC")
                {
                    await DataQueries.SetRoutineData("DATETIME", "last_origin_datetime", PlanModel.MetaData.OriginDate.DateTime);
                    return true;
                }
            }
            catch(Exception ex)
            {
                Logger.Error(LogArea.PlanChecking, ex, "Tried to check if plan is new.");
            }
            return false;
        }
        
    }
}
