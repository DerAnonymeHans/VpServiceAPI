using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Notification;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Repositories
{
    public sealed class ArtworkRepository : IArtworkRepository
    {
        private readonly IDataQueries DataQueries;
        private readonly IMyLogger Logger;

        public ArtworkRepository(IDataQueries dataQueries, IMyLogger logger)
        {
            DataQueries = dataQueries;
            Logger = logger;
        }

        public async Task Add(Artwork artwork)
        {
            await DataQueries.Save("INSERT INTO artwork_data(name, image, color, font_color, start_date, end_date) VALUES (@name, @image, @color, @fontColor, @startDate, @endDate)", new { name = artwork.Name, image = artwork.Image, color = artwork.Color, fontColor=artwork.FontColor.Name.ToLower(), startDate = artwork.Timespan.FromToString(), endDate = artwork.Timespan.ToToString() });
            Logger.Info(LogArea.ArtworkRepo, "Added new Artwork " + artwork.Name);
        }

        private async Task<byte[]> ReadArtworkFile(string name)
        {
            return await File.ReadAllBytesAsync(AppDomain.CurrentDomain.BaseDirectory + $"Pictures/Artworks/{name}.png");
        }

        public async Task<Artwork> Default()
        {
            try
            {
                return (await DataQueries.Load<Artwork, dynamic>("SELECT name, image, color, font_color, start_date, end_date FROM artwork_data WHERE 1 LIMIT 1", new { }))[0];
            }catch(Exception ex)
            {
                Logger.Error(LogArea.ArtworkRepo, ex, "Tried to get default artwork.");
                return new Artwork("rainbow_car", await ReadArtworkFile("rainbow_car"), "red", "white", "0.0.", "0.0.");
            }
        }

        public async Task<ArtworkMeta> DefaultMeta()
        {
            try
            {
                return (await DataQueries.Load<ArtworkMeta, dynamic>("SELECT name, start_date, end_date, color, font_color FROM artwork_data WHERE 1 LIMIT 1", new { }))[0];
            }catch(Exception ex)
            {
                Logger.Error(LogArea.ArtworkRepo, ex, "Tried to get default meta");
                return new ArtworkMeta("rainbow_car", "0.0.", "0.0.", "red", "white");
            }
        }

        public Artwork FormFileToArtwork(IFormFile file, IFormCollection form)
        {
            if (file.Length < 0)
                throw new AppException("Es gibt ein Problem mit der Datei. Die Länge ist kleiner 0");

            using var fileStream = file.OpenReadStream();
            byte[] bytes = new byte[file.Length];
            fileStream.Read(bytes, 0, (int)file.Length);

            var artwork = new Artwork(form["name"], bytes, form["color"], form["fontColor"], form["startDate"], form["endDate"]);
            return artwork;
        }

        public async Task<Artwork> GetArtwork(string name)
        {
            if (!await IncludesArtwork(name)) return await Default();
            try
            {
                return (await DataQueries.Load<Artwork, dynamic>("SELECT name, image, color, font_color, start_date, end_date FROM artwork_data WHERE name=@name", new { name = name }))[0];
            }catch(Exception ex)
            {
                Logger.Error(LogArea.ArtworkRepo, ex, $"Tried to get artwork '{name}'. Now trying to read from disk...");
                try
                {
                    return new Artwork(name, await ReadArtworkFile(name), "red", "white", "0.0.", "0.0.");
                }
                catch(Exception ex2)
                {
                    Logger.Error(LogArea.ArtworkRepo, ex2, $"Tried to read Artwork '{name}' from local disk. Now falling back to local");
                    return await Default();
                }
            }
        }

        public async Task<ArtworkMeta> GetArtworkMeta(string name)
        {
            if (!await IncludesArtwork(name)) return await DefaultMeta();
            try
            {
                return (await DataQueries.Load<ArtworkMeta, dynamic>("SELECT name, start_date, end_date, color, font_color FROM artwork_data WHERE name=@name", new { name = name }))[0];
            }catch(Exception ex)
            {
                Logger.Error(LogArea.ArtworkRepo, ex, $"Tried to get artwork meta '{name}'. Now falling back to default..");
                return new ArtworkMeta(name, "0.0.", "0.0.", "red", "white");
            }
        }

        public async Task<ArtworkMeta?> GetSpecialArtworkForDate(DateTime dateTime)
        {
            var artworkMetaList = new List<ArtworkMeta>();
            try
            {
                artworkMetaList = await DataQueries.Load<ArtworkMeta, dynamic>("SELECT name, start_date, end_date, color, font_color FROM artwork_data WHERE 1", new { });
            }catch(Exception ex)
            {
                Logger.Error(LogArea.ArtworkRepo, ex, "Tried to get artwork meta");
            }

            foreach (var artwork in artworkMetaList)
            {
                if (artwork.Timespan.IncludeAll) continue;
                if (artwork.Timespan.Includes(dateTime))
                {
                    return artwork;
                };
            }
            return null;
        }

        public async Task<bool> IncludesArtwork(string name)
        {
            try
            {
                return (await DataQueries.Load<string, dynamic>("SELECT name FROM artwork_data WHERE name=@name", new { name = name })).Count > 0;
            }catch(Exception ex)
            {
                Logger.Error(LogArea.ArtworkRepo, ex, "Tried to check if artwork is included");
                return false;
            }
        }

    }
}
