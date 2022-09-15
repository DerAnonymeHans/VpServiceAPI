using System.Collections.Generic;
using VpServiceAPI.Entities;
using VpServiceAPI.Entities.Plan;

namespace VpServiceAPI.Interfaces
{
    public interface IGlobalNotificationBody
    {
        public string Subject { get; set; }
        public string GlobalExtra { get; set; }

        public List<GlobalPlan> GlobalPlans { get; set; }

        public ArtworkMeta? Artwork { get; set; }
        public NotificationWeather? Weather { get; set; }
    }

    public interface IGradeNotificationBody
    {
        public string Grade { get; set; }
        public bool IsNotify { get; set; }
        public string? GradeExtra { get; set; }
        public List<List<NotificationRow>> ListOfTables { get; set; }
    }

    public interface IUserNotificationBody
    {
        public string UserName { get; set; }
        public SmallExtra SmallExtra { get; set; }
        public List<string> PersonalInformation { get; }
    }

    public interface INotificationBody : IGlobalNotificationBody, IGradeNotificationBody, IUserNotificationBody
    {
        public INotificationBody Set<T>(T global);
    }
}
