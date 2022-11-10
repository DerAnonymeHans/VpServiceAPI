namespace VpServiceAPI.Entities.Plan
{
    public sealed record PlanRow
    {
        public string Klasse { get; set; } = "";
        public string Stunde { get; set; } = "";
        public string Fach { get; set; } = "";
        public string Lehrer { get; set; } = "";
        public string Raum { get; set; } = "";
        public string Info { get; set; } = "";

        public string[] GetArray()
        {
            return new string[] { Klasse, Stunde, Fach, Lehrer, Raum, Info };
        }
    }
}
