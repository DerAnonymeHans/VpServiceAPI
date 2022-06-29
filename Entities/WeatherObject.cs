using System.Collections.Generic;

namespace VpServiceAPI.Entities
{
    public class WeatherDay
    {
        public string? stationId { get; set; }
        public string dayDate { get; set; }
        public int temperatureMin { get; set; }
        public int temperatureMax { get; set; }
        public int icon { get; set; } = 100;
        public int? icon1 { get; set; }
        public int? icon2 { get; set; }
        public int precipitation { get; set; }
        public int windSpeed { get; set; }
        public int windGust { get; set; }
        public int windDirection { get; set; }
        public int sunshine { get; set; }
    }

}
