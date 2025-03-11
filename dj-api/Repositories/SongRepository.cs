using dj_api.Data;
using dj_api.Models;
using MongoDB.Driver;

namespace dj_api.Repositories
{
    public class SongRepository
    {
        private readonly IMongoCollection<Song> _songsCollection;

        public SongRepository(MongoDbContext dbContext)
        {
            _songsCollection = dbContext.GetCollection<Song>("songs");
        }

        public async Task<List<Song>> GetAllSongsAsync()
        {
            return await _songsCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Song> GetSongByIdAsync(string id)
        {
            return await _songsCollection.Find(song => song.Id == id).FirstOrDefaultAsync();
        }

    }
}
