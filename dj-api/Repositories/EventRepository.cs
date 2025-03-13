using dj_api.Data;
using dj_api.Models;
using MongoDB.Driver;
using QRCoder;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dj_api.Repositories
{
    public class EventRepository
    {
        private readonly IMongoCollection<Event> _eventsCollection;
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(30) // Cache expiration policy
        };

      
        private static HashSet<string> _paginatedCacheKeys = new HashSet<string>();

        public EventRepository(MongoDbContext dbContext, IMemoryCache memoryCache)
        {
            _eventsCollection = dbContext.GetCollection<Event>("DJEvent");
            _memoryCache = memoryCache;
        }

      
        public async Task<List<Event>> GetAllEventsAsync()
        {
            const string cacheKey = "all_events";

            if (!_memoryCache.TryGetValue(cacheKey, out List<Event>? cachedEvents))
            {
                cachedEvents = await _eventsCollection.Find(_ => true).ToListAsync();
                _memoryCache.Set(cacheKey, cachedEvents, _cacheEntryOptions);
            }

            return cachedEvents ?? new List<Event>();
        }

     
        public async Task<Event> GetEventByIdAsync(string id)
        {
            string cacheKey = $"event_{id}";

            if (!_memoryCache.TryGetValue(cacheKey, out Event? cachedEvent))
            {
                cachedEvent = await _eventsCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
                if (cachedEvent != null)
                {
                    _memoryCache.Set(cacheKey, cachedEvent, _cacheEntryOptions);
                }
            }

            return cachedEvent ?? throw new Exception($"Event with ID {id} not found");
        }

      
        public async Task CreateEventAsync(Event eventy)
        {
            var existing = await _eventsCollection.Find(e => e.Id == eventy.Id).FirstOrDefaultAsync();
            if (existing != null)
                throw new Exception($"Event with ID {eventy.Id} already exists");

            await _eventsCollection.InsertOneAsync(eventy);

          
            _memoryCache.Remove("all_events");
            RemovePaginatedEventsCache();
        }

        public async Task DeleteEventAsync(string id)
        {
            var existing = await _eventsCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (existing == null)
                throw new Exception($"Event with ID {id} does not exist");

            await _eventsCollection.DeleteOneAsync(e => e.Id == id);

            // Invalidate cache
            _memoryCache.Remove($"event_{id}");
            _memoryCache.Remove("all_events");
            RemovePaginatedEventsCache();
        }

        public async Task UpdateEventAsync(string id, Event eventy)
        {
            var existingEvent = await _eventsCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (existingEvent == null)
                throw new Exception($"Event with ID {id} does not exist");

            await _eventsCollection.ReplaceOneAsync(e => e.Id == id, eventy);

            _memoryCache.Remove($"event_{id}");
            _memoryCache.Remove("all_events");
            RemovePaginatedEventsCache();
        }

        private void RemovePaginatedEventsCache()
        {
            foreach (var cacheKey in _paginatedCacheKeys)
            {
                _memoryCache.Remove(cacheKey);
            }
            _paginatedCacheKeys.Clear();
        }

     
        public async Task<byte[]> GenerateQRCode(string EventId)
        {
            byte[] qrCodeImg;
            Event eventy = await _eventsCollection.Find(e => e.Id == EventId).FirstOrDefaultAsync();
            if (eventy == null)
                throw new Exception($"Event with ID {EventId} does not exist");

            using (var qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(eventy.QRCodeText, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                qrCodeImg = qrCode.GetGraphic(20);
            }

            return qrCodeImg;
        }


        public async Task<List<Event>> GetPaginatedEventsAsync(int page, int pageSize)
        {
            string cacheKey = $"paginated_events_page_{page}_size_{pageSize}";

            _paginatedCacheKeys.Add(cacheKey);

            if (!_memoryCache.TryGetValue(cacheKey, out List<Event>? cachedEvents))
            {
                cachedEvents = await _eventsCollection
                    .Find(_ => true)
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

              
                cachedEvents ??= new List<Event>();

               
                if (cachedEvents.Count > 0)
                {
                    _memoryCache.Set(cacheKey, cachedEvents, _cacheEntryOptions);
                }
            }

            return cachedEvents ?? new List<Event>();
        }

        public async Task<bool> AddSongToEventAsync(string eventId, Song song, string userId)
        {
            var eventFilter = Builders<Event>.Filter.Eq(e => e.Id, eventId);
            var eventy = await _eventsCollection.Find(eventFilter).FirstOrDefaultAsync();

            if (eventy == null)
            {
                throw new Exception("Event not found.");
            }

            if (eventy.MusicConfig.MusicPlaylist.Any(m => m.MusicName == song.Title))
            {
                return false;
            }

            eventy.MusicConfig.MusicPlaylist.Add(new MusicData
            {
                Id = song.ObjectId,
                MusicName = song.Title,
                Visible = true,
                Votes = 1,
                VotersIDs = new List<string> { userId },
                IsUserRecommendation = true,
                RecommenderID = userId
            });

            var update = Builders<Event>.Update.Set(e => e.MusicConfig, eventy.MusicConfig);
            await _eventsCollection.UpdateOneAsync(eventFilter, update);

            return true;
        }


    }
}
