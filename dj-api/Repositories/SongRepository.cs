using dj_api.Data;
using dj_api.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Caching.Memory;

namespace dj_api.Repositories
{
    public class SongRepository
    {
        private readonly IMongoCollection<Song> _songsCollection;
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };

        public SongRepository(MongoDbContext dbContext, IMemoryCache memoryCache)
        {
            _songsCollection = dbContext.GetCollection<Song>("songs");
            _memoryCache = memoryCache;
        }

        public async Task<Song> GetSongByIdAsync(string id)
        {
            string cacheKey = $"song_{id}";

            if (!_memoryCache.TryGetValue(cacheKey, out Song? cachedSong))
            {
                cachedSong = await _songsCollection.Find(song => song.Id == id).FirstOrDefaultAsync();

                if (cachedSong != null)
                {
                    _memoryCache.Set(cacheKey, cachedSong, _cacheEntryOptions);
                }
            }

            return cachedSong!;
        }
    }
}
