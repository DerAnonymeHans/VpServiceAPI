using System.Collections.Generic;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Entities
{
    public record GlobalNotificationBody : IGlobalNotificationBody
    {
        public string Subject { get; set; } = "Neuer Vertretungsplan!";
        public string GlobalExtra { get; set; } = "";
        public string AffectedDate { get; set; } = "";
        public string AffectedWeekday { get; set; } = "";
        public string OriginDate { get; set; } = "";
        public string OriginTime { get; set; } = "";

        public List<string> Information { get; set; } = new();

        public string AffectedWeekday2 { get; set; } = "";

        public ArtworkMeta? Artwork { get; set; }
        public List<string> MissingTeachers { get; set; } = new();

        public GlobalNotificationBody()
        {

        }
    }

    public record GradeNotificationBody : IGradeNotificationBody
    {
        public string Grade { get; set; } = "";
        public List<NotificationRow> Rows { get; set; } = new();
        public List<NotificationRow> Rows2 { get; set; } = new();
        public bool IsNotify { get; set; }
        public string? GradeExtra { get; set; }
    }

    public record UserNotificationBody : IUserNotificationBody
    {
        public string UserName { get; set; } = "";
        public SmallExtra SmallExtra { get; set; } = new();

        public List<string> PersonalInformation { get; } = new();
    }

    public record NotificationBody : INotificationBody
    {
        public string Subject { get; set; } = "Neuer Vertretungsplan!";
        public string GlobalExtra { get; set; } = "";
        public string AffectedDate { get; set; } = "";
        public string AffectedWeekday { get; set; } = "";
        public string OriginDate { get; set; } = "";
        public string OriginTime { get; set; } = "";

        public string AffectedWeekday2 { get; set; } = "";

        public ArtworkMeta? Artwork { get; set; }
        public List<string> MissingTeachers { get; set; } = new();
        public List<string> Information { get; set; } = new();


        public string Grade { get; set; } = "";
        public List<NotificationRow> Rows { get; set; } = new();
        public List<NotificationRow> Rows2 { get; set; } = new();
        public bool IsNotify { get; set; }
        public string? GradeExtra { get; set; }


        public string UserName { get; set; } = "";
        public SmallExtra SmallExtra { get; set; } = new();
        public List<string> PersonalInformation { get; private set; } = new();

        public INotificationBody Set<T>(T body)
        {
            if (body is null) return this;
            var properties = body.GetType().GetProperties();

            foreach(var prop in properties)
            {
                GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(body));
            }
            return this;
        }
    }

    
}
