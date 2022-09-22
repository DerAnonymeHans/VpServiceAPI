namespace VpServiceAPI.Entities.Tools
{
    public sealed class LogRow
    {
        public string Time { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string Extra { get; set; }

        public LogRow(string time, string type, string message, string extra)
        {
            Time = time;
            Type = type;
            Message = message;
            Extra = string.Join("<br>", extra.Split("|"));
        }

        public override string ToString()
        {
            string color = Type switch
            {
                "ROUTINE" => "hotpink",
                "INFO" => "skyblue",
                "ERROR" => "red",
                "WARN" => "yellow",
                "DEBUG" => "aqua",
                _ => "black"
            };
            return
                $@"
                    <div>
                        <div>
                            <span style=""color: {color}"">{Time}:  </span>
                            <span>{Message}</span>
                        </div>
                        <div>
                            {Extra}
                        </div>
                    </div>
                ";
        }
    }
}
