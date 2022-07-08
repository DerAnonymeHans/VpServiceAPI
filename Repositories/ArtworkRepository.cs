using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VpServiceAPI.Entities;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.Repositories
{
    public class ArtworkRepository : IArtworkRepository
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
            Logger.Info(LogArea.Notification, "Added new Artwork " + artwork.Name);
        }

        public async Task<Artwork> Default()
        {
            return (await DataQueries.Load<Artwork, dynamic>("SELECT name, image, color, font_color, start_date, end_date FROM artwork_data WHERE 1 LIMIT 1", new { }))[0];
        }

        public async Task<ArtworkMeta> DefaultMeta()
        {
            return (await DataQueries.Load<ArtworkMeta, dynamic>("SELECT name, start_date, end_date, color, font_color FROM artwork_data WHERE 1 LIMIT 1", new { }))[0];
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
            return (await DataQueries.Load<Artwork, dynamic>("SELECT name, image, color, font_color, start_date, end_date FROM artwork_data WHERE name=@name", new { name = name }))[0];
        }

        public async Task<ArtworkMeta> GetArtworkMeta(string name)
        {
            if (!await IncludesArtwork(name)) return await DefaultMeta();
            return (await DataQueries.Load<ArtworkMeta, dynamic>("SELECT name, start_date, end_date, color, font_color FROM artwork_data WHERE name=@name", new { name = name }))[0];
        }

        public async Task<ArtworkMeta?> GetSpecialArtworkForDate(DateTime dateTime)
        {
            var artworkMetaList = await DataQueries.Load<ArtworkMeta, dynamic>("SELECT name, start_date, end_date, color, font_color FROM artwork_data WHERE 1", new { });

            foreach (var artwork in artworkMetaList)
            {
                if (artwork.Timespan.IncludeAll) continue;
                if (artwork.Timespan.Includes(dateTime))
                {
                    return await GetArtworkMeta(artwork.Name);
                };
            }
            return null;
        }

        public async Task<bool> IncludesArtwork(string name)
        {
            return (await DataQueries.Load<string, dynamic>("SELECT name FROM artwork_data WHERE name=@name", new { name = name })).Count > 0;
        }

    }
}
