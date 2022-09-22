using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VpServiceAPI.Entities.Notification
{
    public sealed class WeatherDay
    {
        private int tempMin { get; set; }
        private int tempMax { get; set; }
        [JsonPropertyName("stationId")] public string? StationID { get; set; }
        [JsonPropertyName("dayDate")] public string DayDate { get; set; } = "";
        [JsonPropertyName("temperatureMin")]
        public int TemperatureMin
        {
            get
            {
                return tempMin;
            }
            set => tempMin = (int)Math.Round((decimal)value / 10);
        }
        [JsonPropertyName("temperatureMax")]
        public int TemperatureMax
        {
            get
            {
                return tempMax;
            }
            set => tempMax = (int)Math.Round((decimal)value / 10);
        }
        [JsonPropertyName("icon")] public int Icon { get; set; } = 100;
        [JsonPropertyName("icon1")] public int? Icon1 { get; set; }
        [JsonPropertyName("icon2")] public int? Icon2 { get; set; }
        [JsonPropertyName("precipitation")] public int Precipitation { get; set; }
        [JsonPropertyName("windSpeed")] public int WindSpeed { get; set; }
        [JsonPropertyName("windGust")] public int WindGust { get; set; }
        [JsonPropertyName("windDirection")] public int WindDirection { get; set; }
        [JsonPropertyName("sunshine")] public int Sunshine { get; set; }
    }

}
