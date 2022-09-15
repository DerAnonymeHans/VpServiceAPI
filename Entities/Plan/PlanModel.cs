using System;
using System.Collections.Generic;
using System.Threading;

namespace VpServiceAPI.Entities.Plan
{
    public sealed record PlanModel
    {
        public string Title { get; init; }
        public OriginDate OriginDate { get; init; }
        public AffectedDate AffectedDate { get; init; }
        public List<PlanRow> Rows { get; set; } = new();
        public ForceNotifStatus ForceNotifStatus { get; } = new();
        public List<string> Announcements { get; set; } = new();
        public List<string> MissingTeachers { get; set; } = new();

        public PlanModel(string title, OriginDate originDate, AffectedDate affectedDate)
        {
            Title = title;
            OriginDate = originDate;
            AffectedDate = affectedDate;
        }
    }

    public sealed record OriginDate
    {
        public DateTime _dateTime { get; init; }
        public string Date => _dateTime.ToString("dd.MM.yyyy");
        public string Time => _dateTime.ToString("HH:mm");
        public string DateTime => $"{Date}, {Time}";
        public OriginDate(DateTime dateTime)
        {
            _dateTime = dateTime;
        }
    }

    public sealed record AffectedDate
    {
        public DateTime _dateTime { get; init; }
        public string Date => _dateTime.ToString("dd.MM.yyyy");
        public string Weekday
        {
            get
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
                return _dateTime.ToString("dddd");
            }
        }
        public AffectedDate(DateTime dateTime)
        {
            _dateTime = dateTime;
        }
    }

    public sealed record ForceNotifStatus
    {
        public bool IsForce { get; private set; } = false;
        public List<string> Reasons { get; } = new();

        public ForceNotifStatus TrySet(bool force, string reason = "")
        {
            if (!force) return this;
            IsForce = true;
            Reasons.Add(reason);
            return this;
        }
        public ForceNotifStatus TrySet(ForceNotifStatus status)
        {
            if (!status.IsForce) return this;
            IsForce = true;
            Reasons.AddRange(status.Reasons);
            return this;
        }
    }
}
