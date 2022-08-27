using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;
using System;
using System.Text;
using System.IO;
using System.Net;
using System.Diagnostics;
using RestSharp;
using VpServiceAPI.Exceptions;
using System.Threading;

namespace VpServiceAPI.Jobs.Notification
{
    public class TestPushJob : IPushJob
    {
        private readonly IMyLogger Logger;
        private readonly HttpClient Client;
        public TestPushJob(IMyLogger logger)
        {
            Logger = logger;
            Client = new();
        }
        public async Task Push(User user, IGlobalNotificationBody globalBody, IGradeNotificationBody gradeBody)
        {
            var options = new PushOptions(
                "Neuer Vertretungsplan",
                globalBody.Subject,
                $"{Environment.GetEnvironmentVariable("CLIENT_URL")}/Benachrichtigung",
                (long)user.PushId
            )
            {
                Icon = $"{Environment.GetEnvironmentVariable("URL")}/Notification/GetLogo"
            };
            var client = new RestClient("https://api.webpushr.com");
            var request = new RestRequest("v1/notification/send/sid", Method.Post);
            request.AddBody(JsonSerializer.Serialize(options));
            request.AddHeader("webpushrKey", Environment.GetEnvironmentVariable("PUSH_KEY"));
            request.AddHeader("webpushrAuthToken", Environment.GetEnvironmentVariable("PUSH_AUTH"));
            request.AddHeader("Content-Type", "application/json");
            Logger.Info(LogArea.Notification, "Would have sended push Notification to: " + user.Address);
        }
    }

    public class ProdPushJob : IPushJob
    {
        private readonly IMyLogger Logger;
        private readonly HttpClient Client;
        public ProdPushJob(IMyLogger logger)
        {
            Logger = logger;
            Client = new();
        }
        public async Task Push(User user, IGlobalNotificationBody globalBody, IGradeNotificationBody gradeBody)
        {
            var options = new PushOptions(
                "Neuer Vertretungsplan",
                globalBody.Subject,
                $"{Environment.GetEnvironmentVariable("CLIENT_URL")}/Benachrichtigung",
                (long)user.PushId
            )
            {
                Icon = $"{Environment.GetEnvironmentVariable("URL")}/Notification/GetLogo"
            };
            var client = new RestClient("https://api.webpushr.com");
            var request = new RestRequest("v1/notification/send/sid", Method.Post);
            request.AddBody(JsonSerializer.Serialize(options));
            request.AddHeader("webpushrKey", Environment.GetEnvironmentVariable("PUSH_KEY"));
            request.AddHeader("webpushrAuthToken", Environment.GetEnvironmentVariable("PUSH_AUTH"));
            request.AddHeader("Content-Type", "application/json");
            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                throw new AppException($"Status: {response.StatusCode}; Message: {response.Content}");
            }
            Logger.Info(LogArea.Notification, "Send push Notification to: " + user.Address);
            Thread.Sleep(1000);
        }
    }



    class PushOptions
    {
        
        private string title;        
        private string message;
        [JsonPropertyName("target_url")]
        public string TargetUrl { get; set; }
        [JsonPropertyName("sid")]
        public long SID { get; init; }

        public PushOptions(string title, string message, string tagetUrl, long sid)
        {
            Title = title;
            Message = message;
            TargetUrl = tagetUrl;
            SID = sid;
        }
        [JsonPropertyName("title")]
        public string Title {
            get { return title; } 
            set
            {
                title = value.Length > 95 ? value[..95] : value;
            } 
        }
        [JsonPropertyName("message")]
        public string Message {
            get { return message; } 
            set
            {
                message = value.Length > 250 ? value[..250] : value;
            }
        }
        [JsonPropertyName("icon")]
        public string? Icon { get; set; }
        [JsonPropertyName("expire_push")]
        public string? ExpirePush { get; set; }
        [JsonPropertyName("auto_hide")]
        public int? AutoHide { get; set; }
        [JsonPropertyName("send_at")]
        public string? SendAt { get; set; } // 2020-03-04 13:30 -08:00 UTC
        [JsonPropertyName("action_buttons")]
        public List<ActionButton>? ActionButtons { get; set; } 
    }
    class ActionButton
    {
        public ActionButton(string title, string url)
        {
            Title = title;
            URL= url;
        }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("url")]
        public string URL { get; set; }
    }
}
