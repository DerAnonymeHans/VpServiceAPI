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
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;

namespace VpServiceAPI.Jobs.Notification
{
    public sealed class TestPushJob : IPushJob
    {
        private readonly IMyLogger Logger;
        public TestPushJob(IMyLogger logger)
        {
            Logger = logger;
        }

        public async Task Push(User user, PushOptions pushOptions, string reason = "NEWPLAN")
        {
            if (user.PushSubscribtion is null) throw new AppException("User push subscribtion is null");
            var request = new RestRequest("SendPush", Method.Post);
            string json = JsonSerializer.Serialize(new PushBody(user.PushSubscribtion, pushOptions));
            request.AddStringBody(json, DataFormat.Json);

            Logger.Info(LogArea.Notification, $"{reason}: Would have sended push Notification to: " + user.Address);
        }
    }

    public sealed class ProdPushJob : IPushJob
    {
        private readonly IMyLogger Logger;
        private readonly string CLIENT_URL = Environment.GetEnvironmentVariable("CLIENT_URL") ?? "https://kepleraner.herokuapp.com";
        private readonly PushServiceClient PushClient = new();
        private readonly string PUSH_PUB_KEY = Environment.GetEnvironmentVariable("PUSH_PUB_KEY") ?? throw new AppException("PUSH_PUB_KEY is null");
        private readonly string PUSH_PRIV_KEY = Environment.GetEnvironmentVariable("PUSH_PRIV_KEY") ?? throw new AppException("PUSH_PRIV_KEY is null");

        public ProdPushJob(IMyLogger logger)
        {
            Logger = logger;
        }
        public async Task Push(User user, PushOptions pushOptions, string reason = "NEWPLAN")
        {
            var msg = new PushMessage(JsonSerializer.Serialize(pushOptions));
            if(user.PushSubscribtion is null) throw new AppException("User push subscribtion is null");
            var sub = JsonSerializer.Deserialize<PushSubscription>(user.PushSubscribtion, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if(sub is null) throw new AppException("User push subscribtion is invalid");

            var auth = new VapidAuthentication(PUSH_PUB_KEY, PUSH_PRIV_KEY);

            await PushClient.RequestPushMessageDeliveryAsync(sub, msg, auth);
            Logger.Info(LogArea.Notification, $"{reason}: Send push Notification to: " + user.Address);
        }
        //public async Task Push(User user, PushOptions pushOptions, string reason = "NEWPLAN")
        //{
        //    var client = new RestClient(CLIENT_URL);
        //    var request = new RestRequest("SendPush", Method.Post);
        //    if (user.PushSubscribtion is null) throw new AppException("User push subscribtion is null");
        //    string json = JsonSerializer.Serialize(new PushBody(user.PushSubscribtion, pushOptions));
        //    request.AddStringBody(json, DataFormat.Json);

        //    var response = await client.ExecuteAsync(request);
        //    if (!response.IsSuccessful)
        //    {
        //        throw new AppException($"Status: {response.StatusCode}; Message: {response.Content}");
        //    }
        //    Logger.Info(LogArea.Notification, $"{reason}: Send push Notification to: " + user.Address);
        //}
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

    public sealed class PushOptions
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
        public object? Data { get; set; }

        public PushOptions(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }

    public sealed record PushData(string UserName, string Action);
}
