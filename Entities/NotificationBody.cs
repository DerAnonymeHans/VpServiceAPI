using System.Collections.Generic;
using System.Linq;
using VpServiceAPI.Entities.Plan;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Entities
{
    public record GlobalNotificationBody : IGlobalNotificationBody
    {
        public string Subject { get; set; } = "Neuer Vertretungsplan!";
        public string GlobalExtra { get; set; } = "";

        public List<GlobalPlan> GlobalPlans { get; set; } = new();

        public ArtworkMeta? Artwork { get; set; }
        public NotificationWeather? Weather { get; set; }


        public GlobalNotificationBody()
        {

        }
    }

    public record GradeNotificationBody : IGradeNotificationBody
    {
        public string Grade { get; set; } = "";
        public List<List<NotificationRow>> ListOfTables { get; set; } = new();
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

        public List<GlobalPlan> GlobalPlans { get; set; } = new();

        public ArtworkMeta? Artwork { get; set; }
        public NotificationWeather? Weather { get; set; }


        public string Grade { get; set; } = "";
        public List<List<NotificationRow>> ListOfTables { get; set; } = new();
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
                GetType().GetProperty(prop.Name)?.SetValue(this, prop.GetValue(body));
            }
            return this;
        }
    }

    
}
