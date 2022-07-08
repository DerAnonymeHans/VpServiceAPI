using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Tools;

#nullable enable
namespace VpServiceAPI.Jobs.Checking
{
    public class PlanConverterKEPLER : IPlanConverter
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

            var metaData = GetMetaData();
            if(metaData is null)
            {
                Logger.Info(LogArea.PlanConverting, "Quitted conversion of planHTML because metadata coulnt be extracted.");
                return null;
            };

            var table = HTMLIntoRows();
            if(table.Count == 0)
            {
                Logger.Info(LogArea.PlanConverting, "Quitted conversion of planHTML because of lack of rows.");
            }

            return new PlanModel
            {
                MetaData = metaData,
                Table = table
            };
        }
        private bool IsPlan()
        {
            return PlanHTML.IndexOf("<span class=\"ueberschrift\">Geänderte Unterrichtsstunden:</span>") != -1;
        }
        private MetaData? GetMetaData()
        {
            try
            {
                string title = new Regex("(?<=<span class=\"vpfuerdatum\">).*(?=</span>)")
                .Match(PlanHTML).Value;
                string originDateString = new Regex("(?<=<span class=\"vpdatum\">).*(?=</span>)")
                    .Match(PlanHTML).Value;

                var affectedDate = GetAffectedDate(title);
                var originDate = GetOriginDate(originDateString);

                return new MetaData 
                { 
                    Title = title,
                    OriginDate = originDate,
                    AffectedDate = affectedDate,
                };
            }catch (Exception ex)
            {
                Logger.Error(LogArea.PlanConverting, ex, "Tried to extract MetaData.");
                return null;
            }
            
        }
        private AffectedDate GetAffectedDate(string title)
        {
            var dateTime = new DateTime(
                    int.Parse(new Regex("[0-9]+", RegexOptions.RightToLeft).Match(title).Value),
                    Converter.MonthToNumber(new Regex(@"(?<=\d\. )\w+").Match(title).Value),
                    int.Parse(new Regex(@"(?<=\w, )\d+").Match(title).Value)
                );

            return new AffectedDate
            {
                _dateTime = dateTime,
                Date = dateTime.ToString("dd.MM.yyyy")
            };
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

            return new OriginDate
            {
                _dateTime = dateTime,
                DateTime = originDate,
                Date = dateTime.ToString("dd.MM.yyyy"),
                Time = dateTime.ToString("HH:mm")
            };
        }
        private List<PlanRow> HTMLIntoRows()
        {
            string tableHTML = PlanHTML[PlanHTML.IndexOf("<table class=\"tablekopf\"")..];
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

    public class PlanConverterVP24 : IPlanConverter
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

            var metaData = GetMetaData();
            if (metaData is null)
            {
                Logger.Info(LogArea.PlanConverting, "Quitted conversion of planHTML because metadata coulnt be extracted.");
                return null;
            };
            var table = HTMLIntoRows();

            return new PlanModel
            {
                MetaData = metaData,
                Table = table,
                Information = GetInformation(),
                MissingTeacher = GetMissingTeacher()
            };
        }
        private bool IsPlan()
        {
            return PlanHTML.IndexOf("<schulname>Johannes-Kepler-Schule Leipzig</schulname>") != -1;
        }
        private MetaData? GetMetaData()
        {
            try
            {
                string title = XMLParser.GetNodeContent(PlanHTML, "titel");
                string originDateString = XMLParser.GetNodeContent(PlanHTML, "datum");

                var affectedDate = GetAffectedDate(title);
                var originDate = GetOriginDate(originDateString);

                return new MetaData
                {
                    Title = title,
                    OriginDate = originDate,
                    AffectedDate = affectedDate,
                };
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.PlanConverting, ex, "Tried to extract MetaData.");
                return null;
            }

        }
        private AffectedDate GetAffectedDate(string title)
        {
            var dateTime = new DateTime(
                    int.Parse(new Regex(@"[0-9]+(?= \()").Match(title).Value),
                    Converter.MonthToNumber(new Regex(@"(?<=\d\. )\w+").Match(title).Value),
                    int.Parse(new Regex(@"(?<=\w, )\d+").Match(title).Value)
                );

            return new AffectedDate
            {
                _dateTime = dateTime,
                Date = dateTime.ToString("dd.MM.yyyy")
            };
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

            return new OriginDate
            {
                _dateTime = dateTime,
                DateTime = originDate,
                Date = dateTime.ToString("dd.MM.yyyy"),
                Time = dateTime.ToString("HH:mm")
            };
        }
        private List<PlanRow> HTMLIntoRows()
        {
            var tableHTML = XMLParser.GetNodeContent(PlanHTML, "haupt");
            if(tableHTML is null) return new List<PlanRow>();            

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
            if(fussHTML is null) return new List<string>();
            return Regex.Matches(fussHTML, @"(?<=<fussinfo>).+(?=</fussinfo>)")
                .Select(match => match.Value)
                .ToList();
        }
        private List<string>? GetMissingTeacher()
        {
            var missingTeacher = XMLParser.GetNodeContent(PlanHTML, "abwesendl");
            if (missingTeacher is null) return null;
            return missingTeacher.Split(',').ToList();
        }
    }
}
