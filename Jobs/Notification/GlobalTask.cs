using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Entities.Plan;
using VpServiceAPI.Enums;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.Notification
{
    public sealed class GlobalTask
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private readonly IArtworkRepository ArtworkRepository;
        private PlanCollection? PlanCollection { get; set; }

        private readonly HttpClient Client;
        public GlobalTask(IMyLogger logger, IDataQueries dataQueries, IArtworkRepository artworkRepository)
        {
            Logger = logger;
            DataQueries = dataQueries;
            ArtworkRepository = artworkRepository;
            Client = new();
        }

        public async Task<IGlobalNotificationBody> Begin(PlanCollection planCollection)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            PlanCollection = planCollection;

            var weather = await GetWeather();
            var weatherImgName = GetWeatherImageName(weather?.Icon ?? 69);
            NotificationWeather? notifWeather = null;
            if(weather?.TemperatureMax is not null && weather?.TemperatureMin is not null)
            {
                notifWeather = new()
                {
                    TempMin = weather.TemperatureMin,
                    TempMax = weather.TemperatureMax
                };
            }

            return new GlobalNotificationBody
            {
                Subject = planCollection.FirstPlan.Title,
                GlobalExtra = await GetGlobalExtra(),
                Artwork = await GetArtwork(weatherImgName),
                Weather = notifWeather,
                GlobalPlans = planCollection.Plans.Select(plan => new GlobalPlan
                {
                    AffectedDate = plan.AffectedDate.Date,
                    AffectedWeekday = plan.AffectedDate.Weekday,
                    OriginDate = plan.OriginDate.Date,
                    OriginTime = plan.OriginDate.Time,
                    Announcements = plan.Announcements,
                    MissingTeachers = plan.MissingTeachers
                }).ToList()
            };
        }
        private async Task<string> GetGlobalExtra()
        {
            try
            {
                return (await DataQueries.GetRoutineData(RoutineDataSubject.EXTRA, "global_extra"))[0];
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to get global extra.");
                return "";
            }
        }
        
        private async Task<WeatherDay?> GetWeather()
        {
            var response = await FetchWeatherData();
            if (string.IsNullOrEmpty(response))
            {
                Logger.Warn(LogArea.Notification, "Fetch weather data is null.", response);
                return null;
            }
            string json = response;
            return ParseAndGetWeatherDay(json);
        }
        private async Task<string?> FetchWeatherData()
        {
            try
            {
                return await Client.GetStringAsync("https://dwd.api.proxy.bund.dev/v30/stationOverviewExtended?stationIds=10471");
            }catch(Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to fetch weather data.");
                return null;
            }
        }
        private WeatherDay? ParseAndGetWeatherDay(string json)
        {
            try
            {
                int startIdx = json.IndexOf("\"days\"");
                int endIdx = json.IndexOf("],", startIdx);
                json = json.Substring(startIdx + 7, (endIdx + 1) - (startIdx + 7));
                var days = JsonSerializer.Deserialize<List<WeatherDay>>(json);

                if(days is null) throw new Exception("Weatherdays is null");

                foreach (var day in days)
                {
                    var date = new DateTime(
                        int.Parse(day.DayDate.Substring(0, 4)),
                        int.Parse(day.DayDate.Substring(5, 2)),
                        int.Parse(day.DayDate.Substring(8, 2))
                        );
                    if (date.ToString("dd.MM.yyyy") == PlanCollection?.FirstPlan.AffectedDate._dateTime.ToString("dd.MM.yyyy"))
                    {
                        return day;
                    }
                }
                Logger.Warn(LogArea.Notification, "Weather day is null, didnt found matching date", json);
                return null;
            }
            catch(Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to parse weather data", json);
                return null;
            }
        }
        private string GetWeatherImageName(int icon)
        {           
            return icon switch
            {
                1 => "sunny",           // Sonne
                2 => "sunny_bit_cloudy",// Sonne, leicht bewölkt
                3 => "sunny_cloudy",    // Sonne, bewölkt
                4 => "cloudy",          // Wolken
                5 => "foggy",           // Nebel
                6 => "foggy",           // Nebel, rutschgefahr
                7 => "sprinkly",        // leichter Regen
                8 => "rainy",           // Regen
                9 => "rainy",           // starker Regen
                10 => "icy",            // leichter Regen, rutschgefahr
                11 => "icy",            // starker Regen, rutschgefahr
                12 => "rainy",          // Regen, vereinzelt Schneefall
                13 => "snowy",          // Regen, vermehrt Schneefall
                14 => "snowy",          // leichter Schneefall
                15 => "snowy",          // Schneefall
                16 => "snowy",          // starker Schneefall
                17 => "cloudy",         // Wolken, (Hagel)
                18 => "sprinkly",       // Sonne, leichter Regen
                19 => "sunny_cloudy",   // Sonne, starker Regen
                20 => "sunny_cloudy",   // Sonne, Regen, vereinzelter Schneefall
                21 => "snowy",          // Sonne, Regen, vermehrter Schneefall
                22 => "snowy",          // Sonne, vereinzelter Schneefall
                23 => "snowy",          // Sonne, vermehrter Schneefall
                24 => "sunny_cloudy",   // Sonne, (Hagel)
                25 => "hagel",          // Sonne, (staker Hagel)
                26 => "thunder",        // Gewitter
                27 => "thunder",        // Gewitter, Regen
                28 => "thunder",        // Gewitter, starker Regen
                29 => "thunder",        // Gewitter, (Hagel)
                30 => "hagel",          // Gewitter, (starker Hagel)
                31 => "windy",          // Wind
                _ => "no-weather-found"
            };
        }

        private async Task<ArtworkMeta> GetArtwork(string weather)
        {
            string forcedArtworkName = (await DataQueries.GetRoutineData(RoutineDataSubject.EXTRA, "forced_artwork_name"))[0];
            if (!string.IsNullOrEmpty(forcedArtworkName))
            {
                if (await ArtworkRepository.IncludesArtwork(forcedArtworkName))
                {
                    return await ArtworkRepository.GetArtworkMeta(forcedArtworkName);
                }
            }

            ArtworkMeta? specialArtwork = await ArtworkRepository.GetSpecialArtworkForDate(DateTime.Now);
            if (specialArtwork is not null)
            {
                return specialArtwork;
            }

            ArtworkMeta? weatherArtwork = await ArtworkRepository.GetArtworkMeta(weather);
            if (weatherArtwork is not null)
            {
                return weatherArtwork;
            }

            return await ArtworkRepository.DefaultMeta();
        }


    }
}
