﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Jobs.Notification
{
    public sealed class GlobalTask
    {
        private readonly IMyLogger Logger;
        private readonly IDataQueries DataQueries;
        private readonly IArtworkRepository ArtworkRepository;

        private readonly HttpClient Client;
        private PlanModel PlanModel { get; set; } = new();
        public GlobalTask(IMyLogger logger, IDataQueries dataQueries, IArtworkRepository artworkRepository)
        {
            Logger = logger;
            DataQueries = dataQueries;
            ArtworkRepository = artworkRepository;
            Client = new();
        }

        public async Task<IGlobalNotificationBody> Begin(PlanModel planModel)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            PlanModel = planModel;
            var affectedWeekDay2 = PlanModel.MetaData2?.AffectedDate._dateTime.ToString("dddd") ?? "";
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
                AffectedDate = PlanModel.MetaData.AffectedDate.Date,
                AffectedWeekday = PlanModel.MetaData.AffectedDate._dateTime.ToString("dddd"),
                OriginDate = PlanModel.MetaData.OriginDate.Date,
                OriginTime = PlanModel.MetaData.OriginDate.Time,
                AffectedWeekday2 = affectedWeekDay2,
                Subject = PlanModel.MetaData.Title,
                MissingTeachers = PlanModel.MissingTeacher is not null ?  PlanModel.MissingTeacher : await GetMissingTeachers(),
                GlobalExtra = await GetGlobalExtra(),
                Artwork = await GetArtwork(weatherImgName),
                Information = PlanModel.Information,
                Weather = notifWeather
            };
        }
        private async Task<List<string>> GetMissingTeachers()
        {
            try
            {
                return await DataQueries.Load<string, dynamic>("SELECT DISTINCT(`missing_teacher`) FROM `vp_data` WHERE `date`=@date", new { date = PlanModel.MetaData.AffectedDate.Date });
            }
            catch (Exception ex)
            {
                Logger.Error(LogArea.Notification, ex, "Tried to select missing teacher.");
                return new List<string>() { "Es ist ein Fehler aufgetreten :(" };
            }
        }
        private async Task<string> GetGlobalExtra()
        {
            try
            {
                return (await DataQueries.GetRoutineData("EXTRA", "global_extra"))[0];
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
                    if (date.ToString("dd.MM.yyyy") == PlanModel.MetaData.AffectedDate._dateTime.ToString("dd.MM.yyyy"))
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
            string forcedArtworkName = (await DataQueries.GetRoutineData("EXTRA", "forced_artwork_name"))[0];
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
