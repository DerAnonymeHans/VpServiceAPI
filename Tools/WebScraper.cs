using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Tools
{
    public class WebScraper : IWebScraper
    {
        private readonly IMyLogger Logger;
        private readonly HttpClient ClientKepler;
        private readonly HttpClient ClientVP24;
        private readonly HttpClient ClientTextUploader;

        public WebScraper(IMyLogger logger)
        {
            Logger = logger;
            ClientKepler = new();
            ClientVP24 = new();
            ClientTextUploader = new();
        }

        public async Task<string> GetFromKepler(string path)
        {
            string origin = "http://jkg-leipzig.de";
            await ClientKepler.GetAsync(origin);

            Dictionary<string, string> values = new()
            {
                { "userusername", "Schueler" },
                { "userpassword", "HMKwfdn" },
                { "login", "Anmelden" },
                { "redirect", "http://jkg-leipzig.de/wp-admin/" },
                { "option", "ap_user_login" },
            };

            FormUrlEncodedContent data = new(values);

            await ClientKepler.PostAsync(origin + "/wp-login", data);

            return await ClientKepler.GetStringAsync(origin + path);            
        }

        public async Task<string> GetFromTextUploader(string path)
        {
            string origin = "https://textuploader.com";
            string token;
            await ClientTextUploader.GetAsync(origin);
            try
            {
                Console.WriteLine("Hallo");
                string loginPage = await ClientTextUploader.GetStringAsync($"{origin}/auth/login");
                Console.WriteLine(loginPage);
                var form = new XMLParser(loginPage).ExtractNodeContent("body").ExtractNodeContent("form").XML;
                var input = form[form.IndexOf("_token")..];
                int startIdx = input.IndexOf(@"value=""") + 7;
                token = input[startIdx..input.IndexOf("\"", startIdx)];
                if (token.Length == 0) throw new Exception();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                token = "m5n944SA3OGmM0f4ssAIkWQAn67CwdI2HiSAcLUC";
            }

            Dictionary<string, string> values = new()
            {
                { "_token", token },
                { "username", "DerAnonyme" },
                { "password", "12text@uploader07" }
            };

            FormUrlEncodedContent data = new(values);

            await ClientTextUploader.PostAsync(origin + "/auth/login", data);

            return await ClientTextUploader.GetStringAsync(origin + path);
        }

        public async Task<string> GetFromVP24(string path)
        {
            string origin = "https://www.stundenplan24.de/10073128";
            ClientVP24.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "c2NodWVsZXI6MjBKS0dMZWlwemlHMjI=");
            return await ClientVP24.GetStringAsync(origin + path);
        }
    }
}
