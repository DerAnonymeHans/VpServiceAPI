using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.Notification
{
    public class NotificationJob : INotificationJob
    {
        private readonly IMyLogger Logger;
        private readonly INotificator Notificator;
        private readonly IUserRepository UserProvider;
        private readonly IDataQueries DataQueries;
        private readonly INotificationBuilder NotificationBuilder;
        private readonly IArtworkRepository ArtworkRepository;
        private readonly IExtraRepository ExtraRepository;
        private readonly GlobalTask GlobalTask;
        private readonly GradeTask GradeTask;
        private readonly UserTask UserTask;
        private List<User> Users { get; set; } = new();
        private PlanModel PlanModel { get; set; } = new();
        private IGlobalNotificationBody GlobalBody { get; set; } = new GlobalNotificationBody();

        public NotificationJob(
            IMyLogger logger, 
            INotificator notificator,
            IUserRepository userProvider, 
            IDataQueries dataQueries, 
            INotificationBuilder notificationBuilder,
            IArtworkRepository artworkRepository,
            IExtraRepository extraRepository
            )
        {
            Logger = logger;
            Notificator = notificator;
            UserProvider = userProvider;
            DataQueries = dataQueries;
            NotificationBuilder = notificationBuilder;
            ArtworkRepository = artworkRepository;
            ExtraRepository = extraRepository;
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
            GlobalBody = await GlobalTask.Begin(PlanModel);;
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
                var cachedDate = (await DataQueries.GetRoutineData("DATETIME", "last_cache_delete"))[0];
                var today = DateTime.Now.ToString("dd.MM");
                if (cachedDate == today) return;
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
            var prevUser = new User { Name = "$%", Address = "", Grade = "0" };
            IGradeNotificationBody gradeBody = new GradeNotificationBody();
            foreach(User user in Users)
            {
                if(user.Grade != prevUser.Grade)
                {
                    gradeBody = await GradeTask.Begin(PlanModel, user.Grade);
                    Logger.Debug(gradeBody);
                    prevUser = user;
                }

                if (!PlanModel._forceMail)
                {
                    if (!gradeBody.IsSendMail) continue;
                }

                var userBody = await UserTask.Begin(user);

                var notifBody = new NotificationBody();
                notifBody.Set(GlobalBody).Set(gradeBody).Set(userBody);
                notifBody.GlobalExtra = gradeBody.GradeExtra ?? notifBody.GlobalExtra;
                var notification = NotificationBuilder.Build(notifBody, user.Address);
                Notificator.Notify(notification);
            }
        }

        
        

    }
        
}
