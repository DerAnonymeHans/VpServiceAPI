using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VpServiceAPI.Entities
{
    public class WeatherDay
    {
        [JsonPropertyName("stationId")]         public string? StationID { get; set; }
        [JsonPropertyName("dayDate")]           public string DayDate { get; set; } = "";
        [JsonPropertyName("temperatureMin")]    public int TemperatureMin { get; set; }
        [JsonPropertyName("temperatureMax")]    public int TemperatureMax { get; set; }
        [JsonPropertyName("icon")]              public int Icon { get; set; } = 100;
        [JsonPropertyName("icon1")]             public int? Icon1 { get; set; }
        [JsonPropertyName("icon2")]             public int? Icon2 { get; set; }
        [JsonPropertyName("precipitation")]     public int Precipitation { get; set; }
        [JsonPropertyName("windSpeed")]         public int WindSpeed { get; set; }
        [JsonPropertyName("windGust")]          public int WindGust { get; set; }
        [JsonPropertyName("windDirection")]     public int WindDirection { get; set; }
        [JsonPropertyName("sunshine")]          public int Sunshine { get; set; }
    }

}
