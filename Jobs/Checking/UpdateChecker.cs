using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Plan;
using VpServiceAPI.Entities.Tools;
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

        public async Task<StatusWrapper<UpdateCheckStatus, PlanModel>> Check(WhatPlan whatPlan, int dayShift=0)
        {
            var htmlWrapper = await PlanHTMLProvider.GetPlanHTML(GetDate(whatPlan.Number + dayShift));
            if (htmlWrapper.Status == PlanProvideStatus.PLAN_NOT_FOUND) return new(UpdateCheckStatus.NULL, null);
            if(htmlWrapper.Status == PlanProvideStatus.ERROR) return new(UpdateCheckStatus.UNCLEAR, null);

            var planModel = PlanConverter.Convert(htmlWrapper.Body ?? "");
            if (planModel is null) return new(UpdateCheckStatus.UNCLEAR, null);

            if (whatPlan.NotFirst) return new(UpdateCheckStatus.IS_NEW, planModel);

            MyPlan = planModel;
            MyPlan.ForceNotifStatus.TrySet(await GetForceNotifStatus());

            if (!planModel.ForceNotifStatus.IsForce)
            {
                if(!await IsPlanNew()) return new(UpdateCheckStatus.NOT_NEW, null);
            }
            return new(UpdateCheckStatus.IS_NEW, MyPlan);

        }
        // plan for next day after 7 o clock
        // jumps over weekends
        private DateTime GetDate(int dayShift = 0)
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
            return date;
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
