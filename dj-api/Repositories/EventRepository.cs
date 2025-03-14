using dj_api.Data;
using dj_api.Models;
using MongoDB.Driver;
using QRCoder;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using dj_api.ApiModels.Event.Get;
using dj_api.ApiModels.Event.Post;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;

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
                cachedEvent = await _eventsCollection.Find(e => e.ObjectId == id).FirstOrDefaultAsync();
                if (cachedEvent != null)
                {
                    _memoryCache.Set(cacheKey, cachedEvent, _cacheEntryOptions);
                }
            }

            return cachedEvent ?? throw new Exception($"Event with ID {id} not found");
        }

      
        public async Task CreateEventAsync(Event eventy)
        {
            var existing = await _eventsCollection.Find(e => e.ObjectId == eventy.ObjectId).FirstOrDefaultAsync();
            if (existing != null)
                throw new Exception($"Event with ID {eventy.ObjectId} already exists");

            await _eventsCollection.InsertOneAsync(eventy);

          
            _memoryCache.Remove("all_events");
            RemovePaginatedEventsCache();
        }

        public async Task DeleteEventAsync(string id)
        {
           

            await _eventsCollection.DeleteOneAsync(e => e.ObjectId == id);

            
            _memoryCache.Remove($"event_{id}");
            _memoryCache.Remove("all_events");
            RemovePaginatedEventsCache();
        }

        public async Task UpdateEventAsync(string id, Event eventy)
        {
            

            await _eventsCollection.ReplaceOneAsync(e => e.ObjectId == id, eventy);

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
            Event eventy = await _eventsCollection.Find(e => e.ObjectId == EventId).FirstOrDefaultAsync();
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


        public async Task<List<EventGet>> GetPaginatedEventsAsync(int page, int pageSize)
        {
            string cacheKey = $"paginated_events_page_{page}_size_{pageSize}";

            _paginatedCacheKeys.Add(cacheKey);

            if (!_memoryCache.TryGetValue(cacheKey, out List<EventGet>? cachedEvents))
            {
                var projection = Builders<Event>.Projection
             .Include(e => e.ObjectId)
             .Include(e => e.QRCodeText)
             .Include(e => e.DJId)
                .Include(e => e.Name)
                .Include(e => e.Description)
                .Include(e => e.Date)
                .Include(e => e.Location)
                .Include(e => e.Active);



                cachedEvents = await _eventsCollection
                    .Find(_ => true)
                    .Project<EventGet>(projection) 
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                cachedEvents ??= new List<EventGet>();

               
                if (cachedEvents.Count > 0)
                {
                    _memoryCache.Set(cacheKey, cachedEvents, _cacheEntryOptions);
                }
            }

            return cachedEvents ?? new List<EventGet>();
        }

        public async Task<bool> AddSongToEventAsync(string eventId, Song song, string userId)
        {
            var eventFilter = Builders<Event>.Filter.Eq(e => e.Id, eventId);
            var eventy = await _eventsCollection.Find(eventFilter).FirstOrDefaultAsync();

            if (eventy == null)
            {
                throw new Exception("Event not found.");
            }

            if (eventy.MusicConfig.MusicPlaylist.Any(m => m.MusicName == song.Title && m.MusicArtist == song.Artist))
            {
                return false; // Song is already in the event playlist
            }

            eventy.MusicConfig.MusicPlaylist.Add(new MusicData
            {
                ObjectId = song.ObjectId,
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
