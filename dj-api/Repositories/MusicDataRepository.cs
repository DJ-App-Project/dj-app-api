using dj_api.Data;
using dj_api.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dj_api.Repositories
{
    public class MusicDataRepository
    {
        private readonly IMongoCollection<MusicData> _musicDataCollection;

        public MusicDataRepository(MongoDbContext dbContext)
        {
            _musicDataCollection = dbContext.GetCollection<MusicData>("music-data");
        }

        public async Task<List<MusicData>> GetAllMusicDataAsync()
        {
            return await _musicDataCollection.Find(_ => true).ToListAsync();
        }

        public async Task<MusicData> GetMusicDataByIdAsync(string id)
        {
            return await _musicDataCollection.Find(m => m.Id == id).FirstOrDefaultAsync();
        }
        public async Task<List<MusicData>> GetPaginatedMusicAsync(int page, int pageSize)
        {
            return await _musicDataCollection
                .Find(_ => true)  
                .Skip((page - 1) * pageSize) 
                .Limit(pageSize) 
                .ToListAsync();
        }

    }
}
