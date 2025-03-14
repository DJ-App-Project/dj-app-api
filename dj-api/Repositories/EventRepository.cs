using dj_api.Data;
using dj_api.Models;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using QRCoder;
<<<<<<< Updated upstream
=======
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using dj_api.ApiModels.Event.Get;
using dj_api.ApiModels.Event.Post;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Reflection.Metadata.Ecma335;
>>>>>>> Stashed changes

namespace dj_api.Repositories
{
    public class EventRepository
    {
        private readonly IMongoCollection<Event> _eventsCollection;
<<<<<<< Updated upstream
=======
        private readonly IMongoCollection<Song> _songsCollection;
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };
>>>>>>> Stashed changes

        public EventRepository(MongoDbContext dbContext)
        {
            _eventsCollection = dbContext.GetCollection<Event>("DJEvent");
<<<<<<< Updated upstream
=======
            _songsCollection = dbContext.GetCollection<Song>("Songs");
            _memoryCache = memoryCache;
>>>>>>> Stashed changes
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _eventsCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Event> GetEventByIdAsync(string id)
        {
            return await _eventsCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateEventAsync(Event eventy)
        {
            var existing = await _eventsCollection.Find(e => e.Id == eventy.Id).FirstOrDefaultAsync();
            if (existing != null)
                throw new Exception($"Event s {eventy.Id} že obstaja"); // če event že obstaja, vrni Exception

            await _eventsCollection.InsertOneAsync(eventy); // ustvari nov event
        }

        public async Task DeleteEventAsync(string id) // brisanje eventa po ID
        {
            var existing = await _eventsCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (existing == null)
                throw new Exception($"Event s {id} ne obstaja"); // če event ne obstaja, vrni Exception

            await _eventsCollection.DeleteOneAsync(e => e.Id == id);
        }

        public async Task UpdateEventAsync(string id, Event eventy)
        {
            await _eventsCollection.ReplaceOneAsync(e => e.Id == id, eventy); // posodobi event
        }

        //QR Code generacija iz teksta v bazi
        public async Task<Byte[]> GenerateQRCode(string EventId)
        {
            Byte[] qrCodeImg = null;
            Event eventy;

            eventy = await _eventsCollection.Find(e => e.Id == EventId).FirstOrDefaultAsync(); // poišči event po ID
            if (eventy == null)
                throw new Exception($"Event s {EventId} ne obstaja"); // če event ne obstaja, vrni Exception

            using (var qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(eventy.QRCodeText, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                qrCodeImg = qrCode.GetGraphic(20); // generiraj QR kodo iz teksta
            }

<<<<<<< Updated upstream
            return qrCodeImg; // vrni QR kodo
=======
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

            if (eventy.MusicConfig.MusicPlaylist.Any(m =>
                string.Equals(m.MusicName, song.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(m.MusicArtist, song.Artist, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            eventy.MusicConfig.MusicPlaylist.Add(new MusicData
            {
                ObjectId = song.ObjectId,
                MusicName = song.Name,
                Visible = true,
                Votes = 1,
                VotersIDs = new List<string> { userId },
                IsUserRecommendation = true,
                RecommenderID = userId
            });

            var update = Builders<Event>.Update.Set(e => e.MusicConfig, eventy.MusicConfig);
            await _eventsCollection.UpdateOneAsync(eventFilter, update);

            _memoryCache.Set($"event_{eventId}", eventy, _cacheEntryOptions);

            return true;
>>>>>>> Stashed changes
        }

        public async Task<List<Song>> GetSilimarSongsToEvent(string eventId)
        {
            var eventy = await _eventsCollection.Find(e => e.ObjectId == eventId).FirstOrDefaultAsync();

            if (eventy == null)
            {
                return null;
            }
            var playlist = eventy.MusicConfig.MusicPlaylist.ToList();

            if (playlist.Count == 0) //če je playlist prazen, vrni 10 random pesmi
            {
                var similarSongs = await _songsCollection
                   .Find(_ => true)
                   .Limit(10)
                   .ToListAsync();

                return similarSongs;
            }

            var genreCount = new Dictionary<string, int>();

            foreach (var song in playlist)
            {
                if (song.MusicGenre == null) //če je music genre prazen preskočimo
                {
                    continue; 
                }

                if (!genreCount.ContainsKey(song.MusicGenre))
                {
                    genreCount.Add(song.MusicGenre, 1);
                }
                else
                {
                    genreCount[song.MusicGenre]++;
                }
            }

            var leadGenre = "";
            if (genreCount.Count > 0)
            {
                leadGenre = genreCount.OrderByDescending(x => x.Value).First().Key;
                var playlistSongNames = playlist.Select(s => s.MusicName).ToList();
                var similarSongs = await _songsCollection
                   .Find(s => s.Genre == leadGenre && !playlistSongNames.Contains(s.Name))
                   .Limit(10)
                   .ToListAsync();

                return similarSongs; //vrni 10 pesmi iz najbolj popularnega žanra eventa
            }
            else
            {
                var similarSongs = await _songsCollection
                   .Find(_ => true)
                   .Limit(10)
                   .ToListAsync();
                return similarSongs; //če ni žanrov, vrni 10 random pesmi
            }

        }
    }
}
