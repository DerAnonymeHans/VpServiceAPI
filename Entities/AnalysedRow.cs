namespace VpServiceAPI.Entities
{
    public record AnalysedRow
    {
        public int id { get; set; }
        public string type { get; set; } = "";
        public string missing_teacher { get; set; } = "";
        public string substitute_teacher { get; set; } = "";
        public string missing_subject { get; set; } = "";
        public string substitute_subject { get; set; } = "";
        public string lesson { get; set; } = "";
        public string class_name { get; set; } = "";
        public string date { get; set; } = "";
        public string room { get; set; } = "";
        public string extra { get; set; } = "";
    }
}
