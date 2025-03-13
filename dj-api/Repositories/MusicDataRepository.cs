using dj_api.Data;
using dj_api.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dj_api.Repositories
{
    public class MusicDataRepository
    {
        private readonly IMongoCollection<MusicData> _musicDataCollection;
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };

        public MusicDataRepository(MongoDbContext dbContext, IMemoryCache memoryCache)
        {
            _musicDataCollection = dbContext.GetCollection<MusicData>("music-data");
            _memoryCache = memoryCache;
        }

        public async Task<List<MusicData>> GetAllMusicDataAsync()
        {
            const string cacheKey = "all_music_data";

            if (!_memoryCache.TryGetValue(cacheKey, out List<MusicData>? cachedMusicData))
            {
                cachedMusicData = await _musicDataCollection.Find(_ => true).ToListAsync();
                _memoryCache.Set(cacheKey, cachedMusicData, _cacheEntryOptions);
            }

            return cachedMusicData ?? new List<MusicData>();
        }

        public async Task<MusicData> GetMusicDataByIdAsync(string id)
        {
            string cacheKey = $"music_data_{id}";

            if (!_memoryCache.TryGetValue(cacheKey, out MusicData? cachedMusicData))
            {
                cachedMusicData = await _musicDataCollection.Find(m => m.Id == id).FirstOrDefaultAsync();
                if (cachedMusicData != null)
                {
                    _memoryCache.Set(cacheKey, cachedMusicData, _cacheEntryOptions);
                }
            }

            return cachedMusicData ?? throw new Exception($"MusicData with ID {id} not found");
        }

        public async Task<List<MusicData>> GetPaginatedMusicAsync(int page, int pageSize)
        {
            string cacheKey = $"paginated_music_data_page_{page}_size_{pageSize}";

            if (!_memoryCache.TryGetValue(cacheKey, out List<MusicData>? cachedMusicData))
            {
                cachedMusicData = await _musicDataCollection
                    .Find(_ => true)
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                _memoryCache.Set(cacheKey, cachedMusicData, _cacheEntryOptions);
            }

            return cachedMusicData ?? new List<MusicData>();
        }
    }
}
