using System;
using System.Collections.Generic;

namespace VpServiceAPI.Entities
{
    public record OriginDate
    {
        public DateTime _dateTime { get; init; }
        public string DateTime { get; init; } = "";
        public string Date { get; init; } = "";
        public string Time { get; init; } = "";
    }

    public record AffectedDate
    {
        public DateTime _dateTime { get; init; }
        public string Date { get; init; } = "";
    }

    public record MetaData
    {
        public OriginDate OriginDate { get; init; } = new();
        public AffectedDate AffectedDate { get; init; } = new();
        public string Title { get; init; } = "";
    }

    public record PlanModel
    {
        public bool _forceMail { get; set; } = false;
        public MetaData MetaData { get; init; } = new();
        public List<PlanRow> Table { get; init; } = new();

        public List<string> Information { get; init; } = new();
        public List<string>? MissingTeacher { get; init; }

        public MetaData? MetaData2 { get; set; }
        public List<PlanRow>? Table2 { get; set; }
    }
}
