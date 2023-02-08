using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using VpServiceAPI.Interfaces;
using System.Text.Json;
using VpServiceAPI.Enums;
using System.Net.Http;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Entities.Plan;
using VpServiceAPI.Entities.Persons;
using VpServiceAPI.Entities.Notification;

namespace VpServiceAPI.Jobs.Notification
{
    public sealed class NotificationJob : INotificationJob
    {
        private readonly IMyLogger Logger;
        private readonly IEmailJob EmailJob;
        private readonly IUserRepository UserProvider;
        private readonly IDataQueries DataQueries;
        private readonly IEmailBuilder EmailBuilder;
        private readonly IPushJob PushJob;
        private readonly IUserRepository UserRepository;
        private readonly IWebScraper WebScraper;
        private readonly GlobalTask GlobalTask;
        private readonly GradeTask GradeTask;
        private readonly UserTask UserTask;
        private List<User> Users { get; set; } = new();
        private PlanCollection PlanCollection { get; set; } = new();
        private IGlobalNotificationBody GlobalBody { get; set; } = new GlobalNotificationBody();

        public NotificationJob(
            IMyLogger logger,
            IEmailJob notificator,
            IUserRepository userProvider,
            IDataQueries dataQueries,
            IEmailBuilder notificationBuilder,
            IArtworkRepository artworkRepository,
            IExtraRepository extraRepository,
            IPushJob pushJob,
            IUserRepository userRepository,
            IWebScraper webScraper)
        {
            Logger = logger;
            EmailJob = notificator;
            UserProvider = userProvider;
            DataQueries = dataQueries;
            EmailBuilder = notificationBuilder;
            PushJob = pushJob;
            UserRepository = userRepository;
            WebScraper = webScraper;

            GlobalTask = new(logger, dataQueries, artworkRepository);
            GradeTask = new(logger, dataQueries);
            UserTask = new(logger, dataQueries, extraRepository);
        }

        public async void Begin(PlanCollection planCollection)
        {
            PlanCollection = planCollection;
            string lastAffectedDate = await GetLastAffectedDate();

            if(lastAffectedDate != planCollection.FirstPlan.AffectedDate.Date && Environment.GetEnvironmentVariable("VP_SOURCE") != "STATIC")
            {
                await SetLastAffectedDate(planCollection.FirstPlan.AffectedDate.Date);
                await DeleteCache();
                planCollection.ForceNotifStatus.TrySet(true, "new-affected-date");
            }

            await DataQueries.SetRoutineData(RoutineDataSubject.DATETIME, "plan_found_datetime", DateTime.Now.Ticks.ToString());

            Users = await UserProvider.GetUsers();
            GlobalBody = await GlobalTask.Begin(planCollection);
            await CacheGlobalModel(GlobalBody);
            CycleUsers();
        }

        private async Task<string> GetLastAffectedDate()
        {
            try
            {
                return (await DataQueries.GetRoutineData(RoutineDataSubject.DATETIME, "last_affected_date"))[0];
            }catch(Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to get last_affected_date");
                return "";
            }
        }
        private async Task SetLastAffectedDate(string date)
        {
            try
            {
                await DataQueries.SetRoutineData(RoutineDataSubject.DATETIME, "last_affected_date", date);
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to set last_affected_date");
            }
        }
        public async Task DeleteCache()
        {
            try
            {
                var lastCacheDelete = (await DataQueries.GetRoutineData(RoutineDataSubject.DATETIME, "last_cache_delete"))[0];
                var today = DateTime.Now.ToString("dd.MM");
                if (lastCacheDelete == today) return;
                await DataQueries.SetRoutineData(RoutineDataSubject.DATETIME, "last_cache_delete", today);
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to check if cache was already deleted");
            }
            try
            {
                Logger.Info(LogArea.Notification, "Deleting Plan Cache");
                await DataQueries.SetRoutineData(RoutineDataSubject.PLAN_CACHE, null, "");
                //await DataQueries.Save("UPDATE `routine_data` SET `value`='' WHERE `subject`='LAST_PLAN_CACHE'", new { });
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Routine, ex, "Tried to delete LAST_PLAN_CACHE");
            }
        }

        private async void CycleUsers()
        {
            var prevUser = new User(0, "$%&", "", "0", "", "", "", null, null);
            IGradeNotificationBody gradeBody = new GradeNotificationBody();
            var notifBody = new NotificationBody();
            string gradeMailHtml = "";

            if (PlanCollection.ForceNotifStatus.IsForce)
            {
                Logger.Info(LogArea.Notification, "Forcing mail because of: " + string.Join(", ", PlanCollection.ForceNotifStatus.Reasons));
            }

            foreach (User user in Users)
            {
                if(user.Grade != prevUser.Grade)
                {
                    gradeBody = await GradeTask.Begin(PlanCollection, user.Grade);
                    notifBody = new();
                    notifBody.Set(GlobalBody).Set(gradeBody);

                    gradeMailHtml = EmailBuilder.BuildGradeBody(notifBody);
                    await CacheGradeModel(gradeBody);
                    prevUser = user;                                        
                }

                if (!PlanCollection.ForceNotifStatus.IsForce)
                {
                    if (!gradeBody.IsNotify) continue;
                }


                var userBody = await UserTask.Begin(user);

                if (user.NotifyMode == NotifyMode.PWA)
                {
                    if (await TrySendPush(user))
                    {
                        await Task.Delay(200);
                        continue;
                    };
                    string key = user.ResetKey ?? await UserRepository.StartHashResetAndGetKey(user.Address);

                    userBody.PersonalInformation.Add(@$"ACHTUNG: Es wurde versucht dir eine Push Nachticht zu senden, wobei ein Fehler aufkam. Meist liegt die Ursache an fehlenden Benachtichtigungsrechten. Drücke den Link und erlaube sie: <a href=""{Environment.GetEnvironmentVariable("CLIENT_URL")}/Benachrichtigung?code={key}"">Link drücken</a>");
                }
                
                notifBody.Set(userBody);
                notifBody.GlobalExtra = gradeBody.GradeExtra ?? notifBody.GlobalExtra;
                var notification = EmailBuilder.Build(notifBody, user.Address, gradeMailHtml);
                EmailJob.Send(notification);
                await Task.Delay(500);
            }
        }
        private async Task<bool> TrySendPush(User user)
        {
            try
            {
                var pushOptions = new PushOptions("Neuer Vertretungsplan", GlobalBody.Subject)
                {
                    Icon = $"{Environment.GetEnvironmentVariable("URL")}/api/Notification/Logo.png",
                    Badge = $"{Environment.GetEnvironmentVariable("URL")}/api/Notification/Badge_VP.png",
                    Data = new PushData(user.Name, "/Benachrichtigung?page=plan")
                };

                await PushJob.Push(user, pushOptions);
                return true;
            }
            catch (AppException ex)
            {
                Logger.Warn(LogArea.Notification, ex, "Could not send push notification. Sending Email instead", user.Address);
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Could not send push notification. Sending Email instead", user.Address);
            }
            return false;            
        }
        private async Task CacheGlobalModel(IGlobalNotificationBody model)
        {
            await DataQueries.SetRoutineData(RoutineDataSubject.MODEL_CACHE, "global", JsonSerializer.Serialize(model));
        }
        private async Task CacheGradeModel(IGradeNotificationBody model)
        {            
            if (!model.IsNotify && !PlanCollection.ForceNotifStatus.IsForce) return;
            await DataQueries.SetRoutineData(RoutineDataSubject.MODEL_CACHE, model.Grade, JsonSerializer.Serialize(model));
        }
        
    }
        
}
