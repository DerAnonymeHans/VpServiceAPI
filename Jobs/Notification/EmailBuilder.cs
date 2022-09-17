using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace VpServiceAPI.Jobs.Notification
{
    public sealed class EmailBuilder : IEmailBuilder
    {
        private readonly IMyLogger Logger;
        private NotificationBody NotificationBody { get; set; }
        private Dictionary<string, string> HTMLNotificationData { get; set; }

        private readonly string GeneratedPicRoute = $"{Environment.GetEnvironmentVariable("URL")}/Notification/Artwork";
        private readonly string TemplatePath  = AppDomain.CurrentDomain.BaseDirectory + "Templates";
        private string TemplateName { get; set; } = "Default";

        public EmailBuilder(IMyLogger logger)
        {
            Logger = logger;
            NotificationBody = new();
            HTMLNotificationData = new();
        }

        public void ChangeTemplate(string name)
        {
            TemplateName = name;
            Logger.Warn(LogArea.Notification, "Changed Notification Template to:", name);
        }
        public Entities.Notification Build(NotificationBody notificationBody, string receiver, string? template = null)
        {
            NotificationBody = notificationBody;

            return new Entities.Notification
            {
                Receiver = receiver,
                Subject = NotificationBody.Subject,
                Body = BuildBody(template)
            };
        }
        private string BuildBody(string? template = null)
        {
            HTMLNotificationData = GetHTMLNotificationData();
            string HTML = MergeTemplateAndData(template);
            return HTML;
        }
        private Dictionary<string, string> GetHTMLNotificationData()
        {
            var dic =  new Dictionary<string, string>
            {
                { "ArtWorkSrc", @$"{GeneratedPicRoute}/{NotificationBody.Artwork?.Name}" },
                { "Color", NotificationBody.Artwork?.Color ?? "red" },
                { "UserName", NotificationBody.UserName },
                { "Grade", NotificationBody.Grade },
                { "GlobalExtra", NotificationBody.GlobalExtra },
                { "SmallExtra", NotificationBody.SmallExtra.Text },
                { "SmallExtraAuthor", NotificationBody.SmallExtra.Author },
                { "QrCodeSrc", Environment.GetEnvironmentVariable("URL") + "/Notification/Qrcode" },
                { "StatLoginParams", $"stat-user={Environment.GetEnvironmentVariable("SITE_STATS_NAME")}&stat-pw={Environment.GetEnvironmentVariable("SITE_STATS_PW")}" },
                { "PersonalInformation", string.Join("<br>", NotificationBody.PersonalInformation) },
                { "TempMax", NotificationBody.Weather?.TempMax.ToString() ?? ""},
                { "TempMin", NotificationBody.Weather?.TempMin.ToString() ?? ""},
                { "PlansCount", NotificationBody.GlobalPlans.Count.ToString() },
            };


            for(int i=0; i < NotificationBody.GlobalPlans.Count && i < NotificationBody.ListOfTables.Count; i++)
            {
                var anti_i = NotificationBody.GlobalPlans.Count - i - 1;
                var globalPlan = NotificationBody.GlobalPlans[anti_i];
                var table = NotificationBody.ListOfTables[anti_i];

                dic.Add($"AffectedDate{i}", globalPlan.AffectedDate);
                dic.Add($"AffectedWeekday{i}", globalPlan.AffectedWeekday);
                dic.Add($"OriginDate{i}", globalPlan.OriginDate);
                dic.Add($"OriginTime{i}", globalPlan.OriginTime);
                dic.Add($"MissingTeachers{i}", string.Join(", ", globalPlan.MissingTeachers));
                dic.Add($"Announcements{i}", string.Join("<br>", globalPlan.Announcements));
                dic.Add($"PlanRows{i}", GeneratePlanRows(table));
            }

            return dic;
        }
        private string GeneratePlanRows(List<NotificationRow> notifRows)
        {
            List<string> rows = new();

            foreach(NotificationRow row in notifRows)
            {
                List<string> cells = new();
                foreach(string s in row.Row.GetArray())
                {
                    cells.Add($"<td>{s}</td>");
                }

                rows.Add($"<tr class=\"{(row.HasChange ? "row-has-change" : "")}\">{string.Join("", cells)}</tr>");
            }


            return string.Join("", rows);
        }

        private string MergeTemplateAndData(string? template = null)
        {
            string html = template ?? File.ReadAllText(@$"{TemplatePath}/{TemplateName}/index.html");

            int i = 0;
            while(i < 100)
            {
                var match = new Regex(@"\[\[.+\]\]").Match(html);
                if (!match.Success) break;

                string action = new Regex(@"(?<=\[\[)\w+").Match(match.Value).Value;
                string[] parameters = new Regex(@"\w+").Matches(match.Value)
                    .Cast<Match>()
                    .Select(match => match.Value)
                    .Skip(1)
                    .ToArray();

                switch (action)
                {
                    case "if":
                        IfAction(ref html, parameters);
                        break;
                    case "ifnot":
                        IfNotAction(ref html, parameters);
                        break;
                    case "for":
                        LoopAction(ref html, parameters);
                        break;

                }
                i++;
            }
            foreach(Match match in new Regex(@"\[\w+\]").Matches(html))
            {
                string key = match.Value[1..^1];
                if (string.IsNullOrEmpty(HTMLNotificationData[key])) continue;
                html = html.Replace(match.Value, HTMLNotificationData[key]);
            }

            return html;
        }
        private void IfAction(ref string html, string[] parameters)
        {
            string param = parameters[0];
            string _if = $"[[if {param}]]";
            string _endIf = $"[[endif {param}]]";
            // if key and value is present in notif data and not empty
            if (!string.IsNullOrEmpty(HTMLNotificationData[param]))
            {
                // cut [[if ...]] and endif out
                html = html.Replace(_if, "").Replace(_endIf, "");
                return;
            };
            int idxIf = html.IndexOf(_if);
            if (idxIf == -1) return;
            int idxEndIf = html.IndexOf(_endIf);
            string cutout = html.Substring(idxIf, idxEndIf + _endIf.Length - idxIf);
            html = html.Replace(cutout, "");
        }
        private void IfNotAction(ref string html, string[] parameters)
        {
            string param = parameters[0];
            string _ifNot = $"[[ifnot {param}]]";
            string _endIfNot = $"[[endifnot {param}]]";
            // if key and value is not present in notif data and not empty
            if (string.IsNullOrEmpty(HTMLNotificationData[param]))
            {
                // cut [[if ...]] and endif out
                html = html.Replace(_ifNot, "").Replace(_endIfNot, "");
                return;
            };
            int idxIfNot = html.IndexOf(_ifNot);
            if (idxIfNot == -1) return;
            int idxEndIfNot = html.IndexOf(_endIfNot);
            string cutout = html.Substring(idxIfNot, idxEndIfNot + _endIfNot.Length - idxIfNot);
            html = html.Replace(cutout, "");
        }
        private void LoopAction(ref string html, string[] parameters)
        {
            string countVar = parameters[0];
            int count = int.Parse(HTMLNotificationData[countVar]);
            string loopVar = parameters[1];
            string _for = $"[[for {countVar} {loopVar}]]";
            string _endFor = $"[[endfor {loopVar}]]";

            int idx_for = html.IndexOf(_for);
            int idx_endFor = html.IndexOf(_endFor);

            string loopBody = html.Substring(idx_for + _for.Length, idx_endFor - (idx_for + _for.Length));
            html = html.Replace(loopBody, "");

            for(int i=0; i<count; i++)
            {
                html = html.Insert(idx_for + _for.Length, loopBody.Replace(loopVar, i.ToString()));
            }

            html = html.Replace(_for, "").Replace(_endFor, "");
        }
    }

}
