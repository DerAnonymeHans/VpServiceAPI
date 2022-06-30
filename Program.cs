using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VpServiceAPI.Tools;

namespace VpServiceAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production") StartRoutine();
            CreateHostBuilder(args).Build().Run();

        }

        private static async void StartRoutine()
        {
            using(var client = new HttpClient())
            {
                Console.WriteLine("Automatically starting Routine");
                await client.GetAsync($"{Environment.GetEnvironmentVariable("URL")}/Login");

                var values = new Dictionary<string, string>()
                {
                    { "name", Environment.GetEnvironmentVariable("SITE_ADMIN_NAME") },
                    { "pw", Environment.GetEnvironmentVariable("SITE_ADMIN_PW") },

                };
                var content = new FormUrlEncodedContent(values);
                await client.PostAsync($"{Environment.GetEnvironmentVariable("URL")}/login", content);

                await client.PostAsync(Environment.GetEnvironmentVariable("URL") + "/Routine/Begin", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()));
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>            
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
            
    }
}
