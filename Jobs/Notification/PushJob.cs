using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using VpServiceAPI.Interfaces;
using System;
using System.Text;
using System.IO;
using System.Net;
using System.Diagnostics;
using RestSharp;
using VpServiceAPI.Exceptions;
using System.Threading;
using VpServiceAPI.Entities.Persons;

namespace VpServiceAPI.Jobs.Notification
{
    public sealed class TestPushJob : IPushJob
    {
        private readonly IMyLogger Logger;
        private readonly string PUSH_KEY;
        private readonly string PUSH_AUTH_TOKEN;
        public TestPushJob(IMyLogger logger)
        {
            Logger = logger;

            var pushKey = Environment.GetEnvironmentVariable("PUSH_KEY");
            var pushAuthToken = Environment.GetEnvironmentVariable("PUSH_AUTH");

            if (pushKey is null || pushAuthToken is null) Logger.Warn(LogArea.Notification, "Missing PUSH_KEY OR PUSH_AUTH_TOKEN", new {pushKey, pushAuthToken});

            PUSH_KEY = pushKey ?? "";
            PUSH_AUTH_TOKEN = pushAuthToken ?? "";
        }
        public async Task Push(User user, IGlobalNotificationBody globalBody, IGradeNotificationBody gradeBody)
        {
            var options = new PushOptions(
                "Neuer Vertretungsplan",
                globalBody.Subject
            )
            {
                Icon = $"https://vp-service-api.herokuapp.com/Notification/Logo.png"
            };

            var client = new RestClient("https://api.webpushr.com");
            var request = new RestRequest("v1/notification/send/sid", Method.Post);
            request.AddBody(JsonSerializer.Serialize(options));
            request.AddHeader("webpushrKey", PUSH_KEY)
                .AddHeader("webpushrAuthToken", PUSH_AUTH_TOKEN)
                .AddHeader("Content-Type", "application/json");

            Logger.Info(LogArea.Notification, "Would have sended push Notification to: " + user.Address);
            Thread.Sleep(1000); // webpushr only allows push once per second
        }
    }

    public sealed class ProdPushJob : IPushJob
    {
        private readonly IMyLogger Logger;
        private readonly HttpClient HttpClient;
        public ProdPushJob(IMyLogger logger)
        {
            Logger = logger;
            HttpClient = new();
        }
        public async Task Push(User user, IGlobalNotificationBody globalBody, IGradeNotificationBody gradeBody)
        {
            var options = new PushOptions(
                "Neuer Vertretungsplan",
                globalBody.Subject
            )
            {
                Icon = $"{Environment.GetEnvironmentVariable("URL")}/Notification/Logo.png",
                Badge = $"{Environment.GetEnvironmentVariable("URL")}/Notification/Badge.png",
                Data = user.Name,
            };

            var client = new RestClient(Environment.GetEnvironmentVariable("CLIENT_URL"));
            var request = new RestRequest("SendPush", Method.Post);
            string json = JsonSerializer.Serialize(new PushBody(user.PushSubscribtion, options));
            request.AddStringBody(json, DataFormat.Json);

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                throw new AppException($"Status: {response.StatusCode}; Message: {response.Content}");
            }


            Logger.Info(LogArea.Notification, "Send push Notification to: " + user.Address);
        }
    }

    sealed class PushBody
    {
        [JsonPropertyName("subscription")]
        public string Subscribtion { get; set; }

        [JsonPropertyName("options")]
        public PushOptions Options { get; set; }
        public PushBody(string subscribtion, PushOptions options)
        {
            Subscribtion = subscribtion;
            Options = options;
        }

    }

    sealed class PushOptions
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("body")]
        public string Message { get; set; }

        [JsonPropertyName("badge")] // small icon when notification is not expanded (in left upper corner)
        public string? Badge { get; set; }

        [JsonPropertyName("icon")]
        public string? Icon { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }

        public PushOptions(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }
    sealed class ActionButton
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
