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

        public WebScraper(IMyLogger logger)
        {
            Logger = logger;
            ClientKepler = new();
            ClientVP24 = new();
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

        public async Task<string> GetFromVP24(string path)
        {
            string origin = "https://www.stundenplan24.de/10073128";
            ClientVP24.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "c2NodWVsZXI6MjBKS0dMZWlwemlHMjI=");
            return await ClientVP24.GetStringAsync(origin + path);
        }
    }
}
