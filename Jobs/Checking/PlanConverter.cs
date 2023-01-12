using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Plan;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Tools;
using VpServiceAPI.TypeExtensions.String;

#nullable enable
namespace VpServiceAPI.Jobs.Checking
{

    public sealed class PlanConverterVP24 : IPlanConverter
    {
        private readonly IMyLogger Logger;
        private string PlanHTML { get; set; } = "";

        public PlanConverterVP24(IMyLogger logger)
        {
            Logger = logger;
        }
        public PlanModel? Convert(string planHTML)
        {
            PlanHTML = planHTML;
            if (!IsPlan())
            {
                Logger.Info(LogArea.PlanConverting, "Quitted conversion of planHTML because no plan was found.");
                return null;
            }
            try
            {
                var metaData = GetMetaData();

                return new PlanModel(metaData.title, metaData.originDate, metaData.affectedDate)
                {
                    Rows = HTMLIntoRows(),
                    Announcements = GetAnnouncements(),
                    MissingTeachers = GetMissingTeachers()
                };
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.PlanConverting, ex, "Tried to convert plan.");
            }

            return null;

        }
        private bool IsPlan()
        {
            return PlanHTML.IndexOf("<schulname>Johannes-Kepler-Schule Leipzig</schulname>") != -1;
        }
        private (string title, OriginDate originDate, AffectedDate affectedDate) GetMetaData()
        {
            string? title = XMLParser.GetNodeContent(PlanHTML, "titel");
            string? originDateString = XMLParser.GetNodeContent(PlanHTML, "datum");

            if (title is null || originDateString is null) throw new AppException($"VP Title is: {title}. Origin String: {originDateString}");

            var affectedDate = GetAffectedDate(title);
            var originDate = GetOriginDate(originDateString);

            return (title, originDate, affectedDate);

        }
        private AffectedDate GetAffectedDate(string title)
        {
            var dateTime = new DateTime(
                    int.Parse(new Regex(@"[0-9]+(?= \()").Match(title).Value),
                    Converter.MonthToNumber(new Regex(@"(?<=\d\. )\w+").Match(title).Value),
                    int.Parse(new Regex(@"(?<=\w, )\d+").Match(title).Value)
                );

            return new AffectedDate(dateTime);
        }
        private OriginDate GetOriginDate(string originDate)
        {
            var dateTime = new DateTime(
                    int.Parse(new Regex("[0-9]{4}(?=,)").Match(originDate).Value),
                    int.Parse(new Regex(@"(?<=\.)[0-9]{2}(?=\.)").Match(originDate).Value),
                    int.Parse(new Regex(@"^\d{2}").Match(originDate).Value),
                    int.Parse(new Regex(@"\d{2}(?=:)").Match(originDate).Value),
                    int.Parse(new Regex(@"(?<=:)\d{2}").Match(originDate).Value),
                    0
                );

            return new OriginDate(dateTime);
        }
        private List<PlanRow> HTMLIntoRows()
        {
            var tableHTML = XMLParser.GetNodeContent(PlanHTML, "haupt");
            if (tableHTML is null) return new List<PlanRow>();

            var table = new List<PlanRow>();
            for (int i = 0; i < 100; i++)
            {
                var rowStr = XMLParser.GetNode(tableHTML, "aktion");
                if (rowStr is null) break;

                Func<string, string> GetVal = tag => XMLParser.GetNodeContent(rowStr, tag) ?? "";

                var row = new PlanRow
                {
                    Klasse = GetVal("klasse"),
                    Stunde = GetVal("stunde"),
                    Fach = GetVal("fach"),
                    Lehrer = GetVal("lehrer"),
                    Raum = GetVal("raum"),
                    Info = GetVal("info"),
                };

                tableHTML = tableHTML[rowStr.Length..];
                table.Add(row);
            }
            return table;
        }
        private List<string> GetAnnouncements()
        {
            var fussHTML = XMLParser.GetNodeContent(PlanHTML, "fuss");
            if (fussHTML is null) return new List<string>();
            return Regex.Matches(fussHTML, @"(?<=<fussinfo>).+(?=</fussinfo>)")
                .Select(match => match.Value)
                .ToList();
        }
        private List<string> GetMissingTeachers()
        {
            var missingTeacher = XMLParser.GetNodeContent(PlanHTML, "abwesendl");
            if (missingTeacher is null) return new();
            return missingTeacher.Split(',').ToList();
        }
    }


    public sealed class PlanConverterKEPLER : IPlanConverter
    {
        private readonly IMyLogger Logger;
        private readonly IPlanAnalyser PlanAnalyser;
        private string PlanHTML { get; set; } = "";

        public PlanConverterKEPLER(IMyLogger logger, IPlanAnalyser planAnalyser)
        {
            Logger = logger;
            PlanAnalyser = planAnalyser;
        }
        public PlanModel? Convert(string planHTML)
        {
            PlanHTML = planHTML;
            if (!IsPlan())
            {
                Logger.Info(LogArea.PlanConverting, "Quitted conversion of planHTML because no plan was found.");
                return null;
            }
            try
            {
                var metaData = GetMetaData();
                var rows = HTMLIntoRows();
                return new PlanModel(metaData.title, metaData.originDate, metaData.affectedDate)
                {
                    Rows = rows,
                    Announcements = GetAnnouncements(),
                    MissingTeachers = GetMissingTeachers(rows)
                };
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.PlanConverting, ex, "Tried to convert plan.");
            }

            return null;

        }
        private bool IsPlan()
        {
            return PlanHTML.IndexOf("<span class=\"vpfuer\">Vertretungsplan für: </span>") != -1;
        }
        private (string title, OriginDate originDate, AffectedDate affectedDate) GetMetaData()
        {
            string? title = PlanHTML.SubstringSurroundedBy("<span class=\"vpfuerdatum\">", "</span>");
            string? originDateString = PlanHTML.SubstringSurroundedBy("<span class=\"vpdatum\">", "</span>");

            Logger.Debug(title, originDateString);

            if (title is null || originDateString is null) throw new AppException($"VP Title is: {title}. Origin String: {originDateString}");

            var affectedDate = GetAffectedDate(title);
            var originDate = GetOriginDate(originDateString);

            return (title, originDate, affectedDate);

        }
        private AffectedDate GetAffectedDate(string title)
        {
            int year, month, day;
            try
            {
                year = int.Parse(new Regex(@"[0-9]+(?= \()").Match(title).Value);
                month = Converter.MonthToNumber(new Regex(@"(?<=\d\. )\w+").Match(title).Value);
                day = int.Parse(new Regex(@"(?<=\w, )\d+").Match(title).Value);
            }
            catch(Exception ex)
            {
                year = DateTime.Now.Year;
                month = 1;
                day = 1;
                Logger.Error(LogArea.PlanConverting, ex, "Tried to get affected date.");
            }

            var dateTime = new DateTime(
                    year,
                    month,
                    day
                );

            return new AffectedDate(dateTime);
        }
        private OriginDate GetOriginDate(string originDate)
        {
            var dateTime = new DateTime(
                    int.Parse(new Regex("[0-9]{4}(?=,)").Match(originDate).Value),
                    int.Parse(new Regex(@"(?<=\.)[0-9]{2}(?=\.)").Match(originDate).Value),
                    int.Parse(new Regex(@"^\d{2}").Match(originDate).Value),
                    int.Parse(new Regex(@"\d{2}(?=:)").Match(originDate).Value),
                    int.Parse(new Regex(@"(?<=:)\d{2}").Match(originDate).Value),
                    0
                );

            return new OriginDate(dateTime);
        }
        private List<PlanRow> HTMLIntoRows()
        {
            var tableHTML = XMLParser.GetNodeContent(PlanHTML[PlanHTML.IndexOf("<span class=\"ueberschrift\">Geänderte Unterrichtsstunden:</span>")..], "table");
            if (tableHTML is null) return new List<PlanRow>();

            var table = new List<PlanRow>();
            XMLParser.ForEach("tr", tableHTML, (string tr) =>
            {
                if (tr.IndexOf("td") == -1) return "";
                
                int idx = 0;
                var row = new PlanRow();
                XMLParser.ForEach<string?>("td", tr, (string td) =>
                {
                    var val = td.SubstringSurroundedBy(">", "<") ?? "";
                    if (idx == 0) row.Klasse = val;
                    else if (idx == 1) row.Stunde = val;
                    else if (idx == 2) row.Fach = val;
                    else if (idx == 3) row.Lehrer = val;
                    else if (idx == 4) row.Raum = val;
                    else if (idx == 5) row.Info = val;
                    idx++;
                    return null;
                });
                table.Add(row);
                return null;
            }).ToList();            
            return table;
        }
        private List<string> GetAnnouncements()
        {
            var idx = PlanHTML.IndexOf("<span class=\"ueberschrift\">Zusätzliche Informationen:</span>");
            if (idx == -1) return new List<string>();
            var table = XMLParser.GetNodeContent(PlanHTML[idx..], "table");
            if(table is null) return new List<string>();
            return XMLParser.ForEach("td", table, (string td) =>
            {
                var content = td.SubstringSurroundedBy(">", "<");
                return content ?? "";
            }).ToList();
        }
        private List<string> GetMissingTeachers(List<PlanRow> rows)
        {
            var missingTeachers = new List<string>();
            foreach (var row in rows)
            {
                var anaylsedRow = PlanAnalyser.AnalyseRow(row);
                if(anaylsedRow is null) continue;
                if (anaylsedRow.MissingTeacher.Length > 0) missingTeachers.Add(" " + anaylsedRow.MissingTeacher);
            }
            return missingTeachers.Distinct().ToList();
        }
    }

    
}
