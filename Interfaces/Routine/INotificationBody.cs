using System.Collections.Generic;
using VpServiceAPI.Entities;

namespace VpServiceAPI.Interfaces
{
    public interface IGlobalNotificationBody
    {
        public string Subject { get; set; }
        public string GlobalExtra { get; set; }

        public string AffectedDate { get; set; }
        public string AffectedWeekday { get; set; }
        public string OriginDate { get; set; }
        public string OriginTime { get; set; }

        public List<string> Information { get; set; }

        public string AffectedWeekday2 { get; set; }

        public ArtworkMeta? Artwork { get; set; }
        public List<string> MissingTeachers { get; set; }
    }

    public interface IGradeNotificationBody
    {
        public string Grade { get; set; }
        public bool IsSendMail { get; set; }
        public string? GradeExtra { get; set; }
        public List<NotificationRow> Rows { get; set; }
        public List<NotificationRow> Rows2 { get; set; }
    }

    public interface IUserNotificationBody
    {
        public string UserName { get; set; }
        public SmallExtra SmallExtra { get; set; }
    }

    public interface INotificationBody : IGlobalNotificationBody, IGradeNotificationBody, IUserNotificationBody
    {
        public INotificationBody Set<T>(T global);
    }
}
