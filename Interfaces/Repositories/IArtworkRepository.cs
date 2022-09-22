using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using VpServiceAPI.Entities.Notification;

namespace VpServiceAPI.Interfaces
{
    public interface IArtworkRepository
    {
        public Task Add(Artwork artwork);
        public Task<Artwork> GetArtwork(string name);
        public Task<ArtworkMeta> GetArtworkMeta(string name);
        public Task<ArtworkMeta?> GetSpecialArtworkForDate(DateTime dateTime);
        public Task<bool> IncludesArtwork(string name);
        public Task<Artwork> Default();
        public Task<ArtworkMeta> DefaultMeta();
        public Artwork FormFileToArtwork(IFormFile file, IFormCollection form);
    }
}
