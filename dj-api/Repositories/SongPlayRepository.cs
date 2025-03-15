using dj_api.Data;
using dj_api.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;


namespace dj_api.Repositories
{
    public class SongPlayRepository
    {
        private readonly IMongoCollection<SongPlay> _songPlaysCollection;
        private readonly SongRepository _songRepository;

        public SongPlayRepository(MongoDbContext dbContext, SongRepository songRepository)
        {
            _songPlaysCollection = dbContext.GetCollection<SongPlay>("SongPlays");
            _songRepository = songRepository;
        }

        public async Task RecordPlayAsync(string songId)
        {
            var songPlay = new SongPlay
            {
                SongId = songId
            };
            await _songPlaysCollection.InsertOneAsync(songPlay);
        }

        public async Task<Song> GetMostPLayedSongAsync()
        {
            var aggregate = _songPlaysCollection.Aggregate()
                .Group(sp => sp.SongId, g => new { SongId = g.Key, PlayCount = g.Count() })
                .SortByDescending(g => g.PlayCount)
                .Limit(1);

            var result = await aggregate.FirstOrDefaultAsync();
            if (result == null)
                return null;

            return await _songRepository.GetSongByIdAsync(result.SongId);
        }
    }
}
