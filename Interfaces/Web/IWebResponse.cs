namespace VpServiceAPI.Interfaces.Web
{
    public interface IWebResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }
}
