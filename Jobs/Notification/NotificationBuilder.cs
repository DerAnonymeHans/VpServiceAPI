﻿using System;
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
    public class NotificationBuilder : INotificationBuilder
    {
        private readonly IMyLogger Logger;
        private NotificationBody? NotificationBody { get; set; }
        private Dictionary<string, string>? HTMLNotificationData { get; set; }

        private readonly string GeneratedPicRoute = $"{Environment.GetEnvironmentVariable("URL")}/GetArtwork";
        private readonly string TemplatePath  = AppDomain.CurrentDomain.BaseDirectory + "Templates";
        private string TemplateName { get; set; } = "Default";

        public NotificationBuilder(IMyLogger logger)
        {
            Logger = logger;
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
            string HTML = MergeTemplateAndDate();
            return HTML;
        }
        private Dictionary<string, string> GetHTMLNotificationData()
        {
            return new Dictionary<string, string>
            {
                { "ArtWorkSrc", @$"{GeneratedPicRoute}/{NotificationBody.Artwork.Name}/{NotificationBody.UserName}" },
                { "Color", NotificationBody.Artwork.Color },
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
                { "QrCodeSrc", Environment.GetEnvironmentVariable("URL") + "/GetQrcode" },
                { "Information",  GenerateInformation() }

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

        private string MergeTemplateAndDate()
        {
            string HTML = File.ReadAllText(@$"{TemplatePath}/{TemplateName}/index.html");

            foreach(Match match in new Regex(@"\[\[.+\]\]").Matches(HTML))
            {
                string action = new Regex(@"(?<=\[\[)\w+").Match(match.Value).Value;
                string parameter = new Regex(@"\w+(?=\]\])").Match(match.Value).Value;
                switch (action)
                {
                    case "if":
                        string _if = $"[[if {parameter}]]";
                        string _endIf = $"[[endif {parameter}]]";
                        // if key and value is present in notif data and not empty
                        if (!string.IsNullOrEmpty(HTMLNotificationData[parameter]))
                        {
                            // cut [[if ...]] and endif out
                            HTML = HTML.Replace(_if, "").Replace(_endIf, "");                            
                            continue;
                        };
                        int idxIf = HTML.IndexOf(_if);
                        int idxEndIf = HTML.IndexOf(_endIf);
                        string cutout = HTML.Substring(idxIf, idxEndIf + _endIf.Length - idxIf);
                        HTML = HTML.Replace(cutout, "");
                        break;
                }
            }
            foreach(Match match in new Regex(@"\[\w+\]").Matches(HTML))
            {
                string key = match.Value[1..^1];
                HTML = HTML.Replace(match.Value, HTMLNotificationData[key]);
            }


            return HTML;
        }
    }

    public record HTMLNotificationData
    {
        public string ArtWorkSrc { get; set; } = "";
        public string Grade { get; set; } = "";
        public string AffectedDate { get; set; } = "";
        public string OriginDate { get; set; } = "";
        public string OriginTime { get; set; } = "";
        public string MissingTeachers { get; set; } = "";
        public string PlanTable { get; set; } = "";
        public string SmallExtra { get; set; } = "";
        public string SmallExtraAuthor { get; set; } = "";
    }
}
