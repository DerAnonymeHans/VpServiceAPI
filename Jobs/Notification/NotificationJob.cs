using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;
using System.Text.Json;
using VpServiceAPI.Enums;
using System.Net.Http;
using VpServiceAPI.Exceptions;

namespace VpServiceAPI.Jobs.Notification
{
    public class NotificationJob : INotificationJob
    {
        private readonly IMyLogger Logger;
        private readonly IEmailJob Notificator;
        private readonly IUserRepository UserProvider;
        private readonly IDataQueries DataQueries;
        private readonly IEmailBuilder NotificationBuilder;
        private readonly IPushJob PushJob;
        private readonly IUserRepository UserRepository;
        private readonly GlobalTask GlobalTask;
        private readonly GradeTask GradeTask;
        private readonly UserTask UserTask;
        private List<User> Users { get; set; } = new();
        private PlanModel PlanModel { get; set; } = new();
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
            IUserRepository userRepository)
        {
            Logger = logger;
            Notificator = notificator;
            UserProvider = userProvider;
            DataQueries = dataQueries;
            NotificationBuilder = notificationBuilder;
            PushJob = pushJob;
            UserRepository = userRepository;
            GlobalTask = new(logger, dataQueries, artworkRepository);
            GradeTask = new(logger, dataQueries);
            UserTask = new(logger, dataQueries, extraRepository);
        }

        public async void Begin(PlanModel planModel)
        {
            PlanModel = planModel;
            string lastAffectedDate = await GetLastAffectedDate();
            if(lastAffectedDate != planModel.MetaData.AffectedDate.Date && Environment.GetEnvironmentVariable("VP_SOURCE") != "STATIC")
            {
                await SetLastAffectedDate(planModel.MetaData.AffectedDate.Date);
                await DeleteCache();
            }
            Users = await UserProvider.GetUsers();
            GlobalBody = await GlobalTask.Begin(PlanModel);
            await CacheGlobalModel(GlobalBody);
            CycleUsers();
        }

        private async Task<string> GetLastAffectedDate()
        {
            try
            {
                return (await DataQueries.GetRoutineData("DATETIME", "last_affected_date"))[0];
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
                await DataQueries.SetRoutineData("DATETIME", "last_affected_date", date);
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
                var lastCacheDelete = (await DataQueries.GetRoutineData("DATETIME", "last_cache_delete"))[0];
                var today = DateTime.Now.ToString("dd.MM");
                if (lastCacheDelete == today) return;
                await DataQueries.SetRoutineData("DATETIME", "last_cache_delete", today);
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to check if cache was already deleted");
            }
            try
            {
                Logger.Info(LogArea.Notification, "Deleting Plan Cache");
                await DataQueries.SetRoutineData("LAST_PLAN_CACHE", null, "");
                //await DataQueries.Save("UPDATE `routine_data` SET `value`='' WHERE `subject`='LAST_PLAN_CACHE'", new { });
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Routine, ex, "Tried to delete LAST_PLAN_CACHE");
            }
        }

        private async void CycleUsers()
        {
            var prevUser = new User("$%&", "", "0", "", "", "", "");
            IGradeNotificationBody gradeBody = new GradeNotificationBody();
            foreach(User user in Users)
            {
                if(user.Grade != prevUser.Grade)
                {
                    gradeBody = await GradeTask.Begin(PlanModel, user.Grade);
                    await CacheGradeModel(gradeBody);
                    prevUser = user;
                }

                if (!PlanModel._forceNotify)
                {
                    if (!gradeBody.IsNotify) continue;
                }

                var userBody = await UserTask.Begin(user);

                if (user.NotifyMode == NotifyMode.PWA)
                {
                    try
                    {
                        await PushJob.Push(user, GlobalBody, gradeBody);
                        continue;
                    }
                    catch (AppException ex)
                    {
                        Logger.Warn(LogArea.Notification, ex, "Could not send push notification. Sending Email instead", user);
                    }catch(Exception ex)
                    {
                        Logger.Error(LogArea.Notification, ex, "Could not send push notification. Sending Email instead", user);
                    }
                    string key = await UserRepository.StartHashResetAndGetKey(user.Address);
                    userBody.PersonalInformation.Add(@$"Beim Versuch dir eine Push Nachricht zu senden ist ein Problem aufgetreten, weshalb du stattdessen eine Email erhalten hast. Die häufigste Ursache dafür ist, dass du Benachrichtigungen nicht erlaubt hast. Drücke folgenden Link und erlaube anschließend Benachrichtigungen: <a href=""{Environment.GetEnvironmentVariable("CLIENT_URL")}/Benachrichtigung?code={key}"">Link drücken</a>");
                }

                

                var notifBody = new NotificationBody();
                notifBody.Set(GlobalBody).Set(gradeBody).Set(userBody);
                notifBody.GlobalExtra = gradeBody.GradeExtra ?? notifBody.GlobalExtra;
                var notification = NotificationBuilder.Build(notifBody, user.Address);
                Notificator.Send(notification);
            }
        }
        private async Task CacheGlobalModel(IGlobalNotificationBody model)
        {
            await DataQueries.SetRoutineData("MODEL_CACHE", "global", JsonSerializer.Serialize(model));
        }
        private async Task CacheGradeModel(IGradeNotificationBody model)
        {
            await DataQueries.SetRoutineData("MODEL_CACHE", model.Grade, JsonSerializer.Serialize(model));
        }
              
        

    }
        
}
