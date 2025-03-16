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
using System.Reflection.Metadata.Ecma335;
using MongoDB.Bson;

namespace dj_api.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly IMongoCollection<Event> _eventsCollection;
        private readonly IMongoCollection<Song> _songsCollection;
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };

        private static HashSet<string> _paginatedCacheKeys = new HashSet<string>();

        public EventRepository(MongoDbContext dbContext, IMemoryCache memoryCache)
        {
            _eventsCollection = dbContext.GetCollection<Event>("DJEvent");
            _songsCollection = dbContext.GetCollection<Song>("Songs");
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

        public async Task<bool> UpdateEventAsync(string id, Event updatedEvent)
        {
            var updateResult = await _eventsCollection.ReplaceOneAsync(e => e.ObjectId == id, updatedEvent);
            bool success = updateResult.ModifiedCount > 0;

            if (success)
            {
                _memoryCache.Set($"event_{id}", updatedEvent, _cacheEntryOptions);
                _memoryCache.Remove("all_events");
                RemovePaginatedEventsCache();
            }

            return success;
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

            return qrCodeImg; // vrni QR kodo
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

        /// <summary>
        /// Generates awards based on the current state of the event.
        /// </summary>
        /// <param name="currentEvent">The current event.</param>
        /// <returns>A list of awards.</returns>
        public async Task<List<AwardGet>> GenerateAwardsAsync(Event currentEvent)
        {
            return await Task.Run(() =>
            {
                var awards = new List<AwardGet>();

                if (currentEvent.MusicConfig?.MusicPlaylist == null || !currentEvent.MusicConfig.MusicPlaylist.Any())
                {
                    return awards;
                }

                var musicPlaylist = currentEvent.MusicConfig.MusicPlaylist;

                //1 top voted song
                var topVotedSong = musicPlaylist.OrderByDescending(m => m.Votes).FirstOrDefault();

                if (topVotedSong != null)
                {
                    awards.Add(new AwardGet
                    {
                        AwardName = "Top voted song",
                        Description = "The song with the highest number of votes.",
                        SongId = topVotedSong.ObjectId,
                        MusicName = topVotedSong.MusicName,
                        MusicArtist = topVotedSong.MusicArtist,
                        Votes = topVotedSong.Votes
                    });
                }

                //2. most reccomended song

                var mostRecommendedSong = musicPlaylist.Where(m => m.IsUserRecommendation).OrderByDescending(m => m.VotersIDs.Count).FirstOrDefault();

                if (mostRecommendedSong != null)
                {
                    awards.Add(new AwardGet
                    {
                        AwardName = "Most Recommended Song",
                        Description = "The song recommended by the most users.",
                        SongId = mostRecommendedSong.ObjectId,
                        MusicName = mostRecommendedSong.MusicName,
                        MusicArtist = mostRecommendedSong.MusicArtist,
                        Votes = mostRecommendedSong.Votes
                    });
                }

                //3. crowd favorite artist

                var crowdFavoriteArtist = musicPlaylist.Where(m => m.IsUserRecommendation).OrderByDescending(m => m.VotersIDs.Count).FirstOrDefault();

                if (crowdFavoriteArtist != null)
                {
                    awards.Add(new AwardGet
                    {
                        AwardName = "Crowd Favorite Artist",
                        Description = "The artist whose songs received the highest cumulative votes.",
                        MusicName = null,
                        MusicArtist = crowdFavoriteArtist.MusicArtist,
                        Votes = crowdFavoriteArtist.Votes
                    });
                }

                //4. dj's choice

                var djsChoiceSong = musicPlaylist.FirstOrDefault(m => !m.IsUserRecommendation);

                if (djsChoiceSong != null)
                {
                    awards.Add(new AwardGet
                    {
                        AwardName = "DJ's Choice",
                        Description = "A special award for a song selected by the DJ.",
                        SongId = djsChoiceSong.ObjectId,
                        MusicName = djsChoiceSong.MusicName,
                        MusicArtist = djsChoiceSong.MusicArtist,
                        Votes = djsChoiceSong.Votes
                    });
                }

                return awards;
            });
        }

        /// <summary>
        /// Retrieves the top N songs across all events based on total votes.
        /// </summary>
        /// <param name="topN">Number of top songs to retrieve.</param>
        /// <returns>A list of top songs.</returns>
        public async Task<List<TopSongsGet>> GetTopSongsAsync(int topN = 10)
        {
            var events = await _eventsCollection.Find(_ => true).ToListAsync();

            var topSongs = events
                .SelectMany(e => e.MusicConfig?.MusicPlaylist ?? Enumerable.Empty<MusicData>())
                .GroupBy(song => song.MusicGenre)
                .Select(g => new TopSongsGet
                {
                    Genre = g.Key,
                    TotalVotes = g.Sum(song => song.Votes),
                    SongCount = g.Count()
                })
                .OrderByDescending(x => x.TotalVotes)
                .Take(topN)
                .ToList();

            return topSongs;
        }


        /// <summary>
        /// Retrieves genre popularity based on total votes and song count.
        /// </summary>
        /// <returns>A list of genre popularity statistics.</returns>
        public async Task<List<GenrePopularityGet>> GetGenrePopularityAsync()
        {
            var events = await _eventsCollection.Find(_ => true).ToListAsync();

            var genrePopularity = events
                // Use null-conditional operator to default to an empty sequence if MusicConfig or MusicPlaylist is null
                .SelectMany(e => e.MusicConfig?.MusicPlaylist ?? Enumerable.Empty<MusicData>())
                .GroupBy(song => song.MusicGenre)
                .Select(g => new GenrePopularityGet
                {
                    Genre = g.Key,
                    TotalVotes = g.Sum(song => song.Votes),
                    SongCount = g.Select(song => song.MusicName).Distinct().Count()
                })
                .OrderByDescending(x => x.TotalVotes)
                .ToList();

            return genrePopularity;
        }


        /// <summary>
        /// Retrieves user contribution metrics based on recommendations and votes.
        /// </summary>
        /// <returns>A list of user contribution statistics.</returns>
        public async Task<List<UserContributionGet>> GetUserContributionMetricsAsync()
        {
            var events = await _eventsCollection.Find(_ => true).ToListAsync();

            var userContributions = events
                // Safely select MusicPlaylist items; if null, return an empty sequence.
                .SelectMany(e => e.MusicConfig?.MusicPlaylist ?? Enumerable.Empty<MusicData>())
                .GroupBy(song => song.RecommenderID)
                .Select(g => new UserContributionGet
                {
                    UserId = g.Key,
                    Recommendations = g.Sum(song => song.IsUserRecommendation ? 1 : 0),
                    Votes = g.Sum(song => song.Votes)
                })
                .OrderByDescending(x => x.Votes)
                .ToList();

            return userContributions;
        }


        /// <summary>
        /// Retrieves performance metrics for all events.
        /// </summary>
        /// <returns>A list of event performance statistics.</returns>
        public async Task<List<EventPerformanceGet>> GetEventPerformanceMetricsAsync()
        {
            var events = await _eventsCollection.Find(_ => true).ToListAsync();

            var eventPerformances = events
                .Select(e => new EventPerformanceGet
                {
                    EventId = e.ObjectId, // or use e.Id if preferred
                    EventName = e.Name,
                    TotalSongs = e.MusicConfig?.MusicPlaylist?.Count ?? 0,
                    TotalVotes = e.MusicConfig?.MusicPlaylist?.Sum(song => song.Votes) ?? 0,
                    AverageVotesPerSong = (e.MusicConfig?.MusicPlaylist != null && e.MusicConfig.MusicPlaylist.Count > 0)
                        ? e.MusicConfig.MusicPlaylist.Sum(song => song.Votes) / (double)e.MusicConfig.MusicPlaylist.Count
                        : 0
                })
                .OrderByDescending(ep => ep.TotalVotes)
                .ToList();

            return eventPerformances;
        }
        public async Task<List<Event>> FindEvents(string UserId)
        {
            var filter = Builders<Event>.Filter.Eq(e => e.DJId, UserId);
            return await _eventsCollection.Find(filter).ToListAsync();
        }
    }
}

