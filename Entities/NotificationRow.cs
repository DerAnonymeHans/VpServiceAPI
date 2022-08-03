namespace VpServiceAPI.Entities
{
    public record NotificationRow
    {
        public bool HasChange { get; init; }
        public PlanRow Row { get; init; } = new();

        public NotificationRow()
        {

        }
        public NotificationRow(bool has_change, string klasse, string stunde, string fach, string lehrer, string raum, string info)
        {
            HasChange = has_change;
            Row = new PlanRow
            {
                Klasse = klasse,
                Stunde = stunde,
                Fach = fach,
                Lehrer = lehrer,
                Raum = raum,
                Info = info
            };
        }
    }
}
