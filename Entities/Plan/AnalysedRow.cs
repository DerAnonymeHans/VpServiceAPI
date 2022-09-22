using System.Text.Json.Serialization;
using VpServiceAPI.Entities.Tools;

namespace VpServiceAPI.Entities.Plan
{
    public sealed class AnalysedRow : DBEntityBase
    {
        public string Year { get; set; } = "";
        public string Type { get; set; } = "";
        public string MissingTeacher { get; set; } = "";
        public string SubstituteTeacher { get; set; } = "";
        public string MissingSubject { get; set; } = "";
        public string SubstituteSubject { get; set; } = "";
        public string Lesson { get; set; } = "";
        public string ClassName { get; set; } = "";
        public string Date { get; set; } = "";
        public string Room { get; set; } = "";
        public string Extra { get; set; } = "";

        public AnalysedRow(string year, string type, string missing_teacher, string substitute_teacher, string missing_subject, string substitute_subject, string lesson, string class_name, string date, string room, string extra)
        {
            Year = year;
            Type = type;
            MissingTeacher = missing_teacher;
            SubstituteTeacher = substitute_teacher;
            MissingSubject = missing_subject;
            SubstituteSubject = substitute_subject;
            Lesson = lesson;
            ClassName = class_name;
            Date = date;
            Room = room;
            Extra = extra;
        }
        public AnalysedRow()
        {

        }
    }
}
