using System;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Entities.Plan;
using VpServiceAPI.Enums;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.Checking
{
    public sealed class UpdateChecker : IUpdateChecker
    {
        private readonly IPlanHTMLProvider PlanHTMLProvider;
        private readonly IPlanConverter PlanConverter;
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private PlanModel? MyPlan { get; set; }

        public UpdateChecker(IMyLogger logger, IPlanHTMLProvider planHTMLProvider, IDataQueries dataQueries, IPlanConverter planConverter)
        {
            Logger = logger;
            PlanHTMLProvider = planHTMLProvider;
            DataQueries = dataQueries;
            PlanConverter = planConverter;
        }

        public async Task<StatusWrapper<PlanModel>> Check(WhatPlan whatPlan, int dayShift=0)
        {
            var html = await PlanHTMLProvider.GetPlanHTML(whatPlan.Number + dayShift);
            if (string.IsNullOrEmpty(html)) return new StatusWrapper<PlanModel>(Status.NULL, null);

            var planModel = PlanConverter.Convert(html);
            if (planModel is null) return new StatusWrapper<PlanModel>(Status.NULL, null);

            if (whatPlan.NotFirst) return new StatusWrapper<PlanModel>(Status.SUCCESS, planModel);

            MyPlan = planModel;
            MyPlan.ForceNotifStatus.TrySet(await GetForceNotifStatus());

            if (!planModel.ForceNotifStatus.IsForce)
            {
                if(!await IsPlanNew()) return new StatusWrapper<PlanModel>(Status.FAIL, null);
            }
            return new StatusWrapper<PlanModel>(Status.SUCCESS, MyPlan);

        }
        private async Task<ForceNotifStatus> GetForceNotifStatus()
        {
            bool isForceOnInfoChange = false;
            var forcePlanStatus = new ForceNotifStatus();
            try
            {
                isForceOnInfoChange = (await DataQueries.GetRoutineData(RoutineDataSubject.FORCE_MODE, "on_info_change"))[0] == "true";
            }catch(Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to determine if mail should be force to send");
            }

            if (isForceOnInfoChange)
            {
                if (await IsInfoChange()) return forcePlanStatus.TrySet(true, "announcement-change");
            }
            return forcePlanStatus;
        }
        private async Task<bool> IsInfoChange()
        {
            if (MyPlan is null) return false;
            try
            {
                var cached = (await DataQueries.GetRoutineData(RoutineDataSubject.CACHE, "information"))[0];
                if (cached != string.Join('|', MyPlan.Announcements))
                {
                    await DataQueries.SetRoutineData(RoutineDataSubject.CACHE, "information", string.Join('|', MyPlan.Announcements));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to compare cached 'information' with planmodel informaion.", MyPlan.Announcements);
            }
            return false;
        }
        private async Task<bool> IsPlanNew()
        {
            if (MyPlan is null) return false; // wont happen
            if (Environment.GetEnvironmentVariable("MODE") == "Testing" || Environment.GetEnvironmentVariable("VP_SOURCE") == "STATIC") return true;
            try
            {
                var lastPlanTime = (await DataQueries.GetRoutineData(RoutineDataSubject.DATETIME, "last_origin_datetime"))[0];
                var lastAffectedDate = (await DataQueries.GetRoutineData(RoutineDataSubject.DATETIME, "last_affected_date"))[0];
                if((lastPlanTime != MyPlan.OriginDate.DateTime || lastAffectedDate != MyPlan.AffectedDate.Date) 
                    && Environment.GetEnvironmentVariable("VP_SOURCE") != "STATIC")
                {
                    await DataQueries.SetRoutineData(RoutineDataSubject.DATETIME, "last_origin_datetime", MyPlan.OriginDate.DateTime);
                    // last_affected_date get set at NotificationJob
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
