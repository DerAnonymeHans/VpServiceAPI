using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Plan;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Tools;

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
                    Announcements = GetInformation(),
                    MissingTeachers = GetMissingTeacher()
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
                var rowStr = XMLParser.GetNodeContent(tableHTML, "aktion");
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
        private List<string> GetInformation()
        {
            var fussHTML = XMLParser.GetNodeContent(PlanHTML, "fuss");
            if (fussHTML is null) return new List<string>();
            return Regex.Matches(fussHTML, @"(?<=<fussinfo>).+(?=</fussinfo>)")
                .Select(match => match.Value)
                .ToList();
        }
        private List<string> GetMissingTeacher()
        {
            var missingTeacher = XMLParser.GetNodeContent(PlanHTML, "abwesendl");
            if (missingTeacher is null) return new();
            return missingTeacher.Split(',').ToList();
        }
    }


    public sealed class PlanConverterKEPLER : IPlanConverter
    {
        private readonly IMyLogger Logger;
        private string PlanHTML { get; set; } = "";

        public PlanConverterKEPLER(IMyLogger logger)
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
                    Rows = HTMLIntoRows()
                };
            }catch(Exception ex)
            {
                Logger.Error(LogArea.PlanConverting, ex, "Tried to convert plan.");
            }

            return null;
        }
        private bool IsPlan()
        {
            return PlanHTML.IndexOf("<span sealed class=\"ueberschrift\">Geänderte Unterrichtsstunden:</span>") != -1;
        }
        private (string title, OriginDate originDate, AffectedDate affectedDate) GetMetaData()
        {
            string title = new Regex("(?<=<span sealed class=\"vpfuerdatum\">).*(?=</span>)")
            .Match(PlanHTML).Value;
            string originDateString = new Regex("(?<=<span sealed class=\"vpdatum\">).*(?=</span>)")
                .Match(PlanHTML).Value;

            var affectedDate = GetAffectedDate(title);
            var originDate = GetOriginDate(originDateString);

            return (title, originDate, affectedDate);
        }
        private AffectedDate GetAffectedDate(string title)
        {
            var dateTime = new DateTime(
                    int.Parse(new Regex("[0-9]+", RegexOptions.RightToLeft).Match(title).Value),
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
            string tableHTML = PlanHTML[PlanHTML.IndexOf("<table sealed class=\"tablekopf\"")..];
            tableHTML = tableHTML[..tableHTML.IndexOf("</table>")];

            var table = new List<PlanRow>();
            for(int i=0; i<100; i++)
            {
                // selects <tr> row
                int trStartIdx = tableHTML.IndexOf("<tr");
                int trEndIdx = tableHTML.IndexOf("</tr>");
                if (trStartIdx == -1) break;
                string rowStr = tableHTML.Substring(trStartIdx + 4, trEndIdx - trStartIdx);

                // matches all >..</td> in the row, loops through them and selects the content
                string[] rowArr = new Regex("(?<=>).*(?=</td>)").Matches(rowStr)
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .ToArray();

                tableHTML = tableHTML[(trEndIdx + 5)..];
                if (rowArr.Length == 0 || rowArr is null) continue;
                table.Add(new PlanRow
                {
                    Klasse = rowArr[0],
                    Stunde = rowArr[1],
                    Fach = rowArr[2],
                    Lehrer = rowArr[3],
                    Raum = rowArr[4],
                    Info = rowArr[5],
                });
            }

            return table;            
        }
    }

    
}
