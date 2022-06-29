using System.Threading.Tasks;

namespace VpServiceAPI.Interfaces
{
    public interface IWebScraper
    {
        public Task<string> GetFromKepler(string path);
        public Task<string> GetFromVP24(string path);

    }
}
