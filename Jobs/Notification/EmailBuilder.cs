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
        public Entities.Notification Build(NotificationBody notificationBody, string receiver)
        {
            NotificationBody = notificationBody;

            return new Entities.Notification
            {
                Receiver = receiver,
                Subject = NotificationBody.Subject,
                Body = BuildBody()
            };
        }
        private string BuildBody()
        {
            HTMLNotificationData = GetHTMLNotificationData();
            string HTML = MergeTemplateAndData();
            return HTML;
        }
        private Dictionary<string, string> GetHTMLNotificationData()
        {

            return new Dictionary<string, string>
            {
                { "ArtWorkSrc", @$"{GeneratedPicRoute}/{NotificationBody.Artwork?.Name}/{NotificationBody.UserName}" },
                { "Color", NotificationBody.Artwork?.Color ?? "red" },
                { "UserName", NotificationBody.UserName },
                { "Grade", NotificationBody.Grade },
                { "AffectedDate", NotificationBody.AffectedDate },
                { "AffectedWeekday", NotificationBody.AffectedWeekday},
                { "AffectedWeekday2", NotificationBody.AffectedWeekday2},
                { "OriginDate", NotificationBody.OriginDate },
                { "OriginTime", NotificationBody.OriginTime },
                { "GlobalExtra", NotificationBody.GlobalExtra },
                { "MissingTeachers", string.Join(", ", NotificationBody.MissingTeachers) },
                { "PlanRows", GeneratePlanRows() },
                { "SecondPlanRows", GeneratePlanRows(true) },
                { "SmallExtra", NotificationBody.SmallExtra.Text },
                { "SmallExtraAuthor", NotificationBody.SmallExtra.Author },
                { "QrCodeSrc", Environment.GetEnvironmentVariable("URL") + "/Notification/Qrcode" },
                { "Information",  GenerateInformation() },
                { "StatLoginParams", $"stat-user={Environment.GetEnvironmentVariable("SITE_STATS_NAME")}&stat-pw={Environment.GetEnvironmentVariable("SITE_STATS_PW")}" },
                { "PersonalInformation", string.Join("<br>", NotificationBody.PersonalInformation) },
                { "TempMax", NotificationBody.Weather?.TempMax.ToString() ?? ""},
                { "TempMin", NotificationBody.Weather?.TempMin.ToString() ?? ""},

            };
        }
        private string GeneratePlanRows(bool isSecondPlan=false)
        {
            List<string> rows = new();

            var table = isSecondPlan ? NotificationBody.Rows2 : NotificationBody.Rows;

            foreach(NotificationRow row in table)
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
        private string GenerateInformation()
        {
            List<string> divs = new();
            foreach(string info in NotificationBody.Information)
            {
                divs.Add($"<div>{info}</div>");
            }
            return string.Join("", divs);
        }

        private string MergeTemplateAndData()
        {
            string html = File.ReadAllText(@$"{TemplatePath}/{TemplateName}/index.html");

            foreach(Match match in new Regex(@"\[\[.+\]\]").Matches(html))
            {
                string action = new Regex(@"(?<=\[\[)\w+").Match(match.Value).Value;
                string parameter = new Regex(@"\w+(?=\]\])").Match(match.Value).Value;

                switch (action)
                {
                    case "if":
                        IfAction(ref html, parameter);
                        break;
                    case "ifnot":
                        IfNotAction(ref html, parameter);
                        break;

                }
            }
            foreach(Match match in new Regex(@"\[\w+\]").Matches(html))
            {
                string key = match.Value[1..^1];
                html = html.Replace(match.Value, HTMLNotificationData[key]);
            }

            return html;
        }
        private void IfAction(ref string html, string parameter)
        {
            string _if = $"[[if {parameter}]]";
            string _endIf = $"[[endif {parameter}]]";
            // if key and value is present in notif data and not empty
            if (!string.IsNullOrEmpty(HTMLNotificationData[parameter]))
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
        private void IfNotAction(ref string html, string parameter)
        {
            string _ifNot = $"[[ifnot {parameter}]]";
            string _endIfNot = $"[[endifnot {parameter}]]";
            // if key and value is not present in notif data and not empty
            if (string.IsNullOrEmpty(HTMLNotificationData[parameter]))
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
    }

}
