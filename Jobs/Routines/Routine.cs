using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using VpServiceAPI.Entities;
using VpServiceAPI.Entities.Plan;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Jobs.Checking;
using VpServiceAPI.Jobs.Notification;

namespace VpServiceAPI.Jobs.Routines
{
    public sealed class Routine : IRoutine
    {
        private readonly IMyLogger Logger;
        private readonly IUpdateChecker UpdateChecker;
        private readonly IPlanAnalyser AnalysePlanJob;
        private readonly IAnalysedPlanSaver AnalysedPlanSaver;
        private readonly INotificationJob NotificationJob;
        private readonly IStatExtractor StatExtractor;
        private readonly ITeacherRepository TeacherRepository;

        public bool IsRunning { get; private set; }
        public int Interval { get; private set; } = 600_000;

        private Timer Timer { get; set; } = new Timer();

        public Routine(
            IMyLogger logger,
            IUpdateChecker updateChecker,
            IPlanAnalyser analysePlanJob,
            IAnalysedPlanSaver analysedPlanSaver,
            INotificationJob notificationJob,
            IStatExtractor statExtractor,
            ITeacherRepository teacherRepository)
        {
            Logger = logger;
            UpdateChecker = updateChecker;
            AnalysePlanJob = analysePlanJob;
            AnalysedPlanSaver = analysedPlanSaver;
            NotificationJob = notificationJob;
            StatExtractor = statExtractor;
            TeacherRepository = teacherRepository;
        }
        public void Begin()
        {
            if (IsRunning) throw new AppException("Timer is already running");
            Timer.Dispose();
            Timer = new();

            Timer.Interval = Interval;
            Timer.Elapsed += OnTimed;

            Timer.Start();
            IsRunning = true;

            Logger.Info(LogArea.Routine, "Starting Routine");

            DoJob();
        }
        public void ChangeInterval(int interval)
        {
            Interval = interval;
            Logger.Info(LogArea.Routine, "Changing Routine Interval", interval);
            Timer.Interval = interval;
        }
        public void End()
        {
            Timer.Stop();
            Logger.Info(LogArea.Routine, "Stopped Routine");
            IsRunning = false;
        }
        private void OnTimed(Object source, ElapsedEventArgs e)
        {
            Logger.Routine("Update-Checker & Notificator");
            DoJob();
        }

        public void BeginOnce()
        {
            Logger.Info("Update-Checker & Notificator");
            DoJob();
        }

        private async void DoJob()
        {            
            try
            {
                await TimedEvents();

                PlanCollection planCollection = new();
                var whatPlan = new WhatPlan(0);
                int MAX_PLAN_COUNT = 5;

                for(int dayShift = 0; dayShift < 5; dayShift++)
                {
                    var wrapper = await UpdateChecker.Check(whatPlan, dayShift);
                    if (wrapper.Status == Status.SUCCESS)
                    {
                        planCollection.Add(wrapper.Body ?? throw new AppException("Status is success but body is null"));
                        whatPlan.NextPlan();
                        for (; whatPlan.Number < MAX_PLAN_COUNT; whatPlan.NextPlan())
                        {
                            wrapper = await UpdateChecker.Check(whatPlan, dayShift);
                            if (wrapper.Status == Status.NULL) break;
                            planCollection.Add(wrapper.Body ?? throw new AppException("Status is success but body of not first plan is null"));
                        }
                        break;
                    }
                    if (wrapper.Status == Status.FAIL) return; // when first plan is not nofify-worthy
                }

                if (planCollection.Plans.Count == 0) return;
                                
                Logger.Info($"{planCollection.Plans.Count} NEW PLAN{(planCollection.Plans.Count == 1 ? "" : "S")} : <br>\n" + string.Join("<br>\n", planCollection.Plans.Select(plan => plan.Title)));

                var analysedRows = AnalysePlanJob.Analyse(planCollection.FirstPlan);
                if (Environment.GetEnvironmentVariable("VP_SOURCE") != "STATIC")
                {
                    await AnalysedPlanSaver.DeleteOldRows(planCollection.FirstPlan.AffectedDate.Date);
                    await AnalysedPlanSaver.SaveRows(analysedRows);
                }

                NotificationJob.Begin(planCollection);

            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Routine, ex, "Failed on Routine.");
            }
        }

        private async Task TimedEvents()
        {
            var now = DateTime.Now;
            if(TeacherRepository.ShouldUpdateTeacherList)
            {
                await TeacherRepository.UpdateTeacherList();
            }
            if (now.Hour > 16)
            {
                if(Environment.GetEnvironmentVariable("GEN_STATS") != "false") await StatExtractor.Begin(DateTime.Now);
            }
        } 
    }
}
