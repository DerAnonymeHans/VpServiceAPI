using VpServiceAPI.Interfaces.Web;

namespace VpServiceAPI.WebResponse
{
    public record WebResponse<TBody> : IWebResponse
    {
        public bool IsSuccess { get; set; } = true;
        public string? Message { get; set; }
        public TBody? Body { get; set; }
    }
    public record WebMessage : IWebResponse
    {
        public bool IsSuccess { get; set; } = true;
        public string? Message { get; set; }
        public string? Body { get; } = null;
    }
}
