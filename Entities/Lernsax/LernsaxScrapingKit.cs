using System.Net.Http;

namespace VpServiceAPI.Entities.Lernsax
{
    public sealed record LernsaxScrapingKit(bool LoginSuccesfull, HttpClient Client, string StartHtml);
}
