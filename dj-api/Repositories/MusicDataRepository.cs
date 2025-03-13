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

   
        private static HashSet<string> _paginatedCacheKeys = new HashSet<string>();

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

        public async Task CreateMusicDataAsync(MusicData musicData)
        {
            var existing = await _musicDataCollection.Find(m => m.Id == musicData.Id).FirstOrDefaultAsync();
            if (existing != null)
                throw new Exception($"MusicData with ID {musicData.Id} already exists");

            await _musicDataCollection.InsertOneAsync(musicData);

            
            _memoryCache.Remove("all_music_data");
            RemovePaginatedMusicDataCache();
        }

        public async Task DeleteMusicDataAsync(string id)
        {
            await _musicDataCollection.DeleteOneAsync(m => m.Id == id);

       
            _memoryCache.Remove($"music_data_{id}");
            _memoryCache.Remove("all_music_data");
            RemovePaginatedMusicDataCache();
        }

        public async Task UpdateMusicDataAsync(string id, MusicData musicData)
        {
            await _musicDataCollection.ReplaceOneAsync(m => m.Id == id, musicData);

          
            _memoryCache.Remove($"music_data_{id}");
            _memoryCache.Remove("all_music_data");
            RemovePaginatedMusicDataCache();
        }

        private void RemovePaginatedMusicDataCache()
        {
            foreach (var cacheKey in _paginatedCacheKeys)
            {
                _memoryCache.Remove(cacheKey);
            }

            _paginatedCacheKeys.Clear();
        }

        public async Task<List<MusicData>> GetPaginatedMusicDataAsync(int page, int pageSize)
        {
            string cacheKey = $"paginated_music_data_page_{page}_size_{pageSize}";

            _paginatedCacheKeys.Add(cacheKey);

            if (!_memoryCache.TryGetValue(cacheKey, out List<MusicData>? cachedMusicData))
            {
                cachedMusicData = await _musicDataCollection
                    .Find(_ => true)
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                cachedMusicData ??= new List<MusicData>();

             
                if (cachedMusicData.Any())
                {
                    _memoryCache.Set(cacheKey, cachedMusicData, _cacheEntryOptions);
                }
            }

            return cachedMusicData;
        }

    }
}
