namespace VpServiceAPI.WebResponse
{
    public record WebResponse<TBody>
    {
        public bool IsSuccess { get; set; } = true;
        public string? Message { get; set; }
        public TBody? Body { get; set; }
    }
    public record WebMessage
    {
        public bool IsSuccess { get; set; } = true;
        public string? Message { get; set; }
        public string? Body { get; } = null;
    }
}
