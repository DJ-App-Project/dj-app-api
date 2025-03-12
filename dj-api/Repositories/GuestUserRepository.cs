using dj_api.Data;
using dj_api.Models;
using MongoDB.Driver;

namespace dj_api.Repositories
{
    public class GuestUserRepository
    {
        private readonly IMongoCollection<GuestUser> _guestUsersCollection;

        public GuestUserRepository(MongoDbContext dbContext)
        {
            _guestUsersCollection = dbContext.GetCollection<GuestUser>("GuestUser");
        }

        public async Task<List<GuestUser>> GetAllUsersAsync()
        {
            return await _guestUsersCollection.Find(_ => true).ToListAsync();
        }

        public async Task<GuestUser> GetUserByIdAsync(string id)
        {
            return await _guestUsersCollection.Find(user => user.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(GuestUser user)
        {
            await _guestUsersCollection.InsertOneAsync(user);
        }

        public async Task DeleteUserAsync(string id)
        {
            await _guestUsersCollection.DeleteOneAsync(user => user.Id == Convert.ToInt32(id));
        }

        public async Task UpdateUserAsync(string id, GuestUser user)
        {
            await _guestUsersCollection.ReplaceOneAsync(user => user.Id == Convert.ToInt32(id), user);
        }

    }
}
