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
            var existing = await _guestUsersCollection.Find(user => user.Id == user.Id).FirstOrDefaultAsync();
            if (existing != null)
                throw new Exception($"User s {user.Id} že obstaja"); // če uporabnik že obstaja, vrni Exception

            await _guestUsersCollection.InsertOneAsync(user); // ustvari novega uporabnika
        }

        public async Task DeleteUserAsync(string id)
        {
            var existing = await _guestUsersCollection.Find(user => user.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
            if (existing == null)
                throw new Exception($"User s {id} ne obstaja"); // če uporabnik ne obstaja, vrni Exception

            await _guestUsersCollection.DeleteOneAsync(user => user.Id == Convert.ToInt32(id)); // izbriši uporabnika
        }

        public async Task UpdateUserAsync(string id, GuestUser user)
        {
            await _guestUsersCollection.ReplaceOneAsync(user => user.Id == Convert.ToInt32(id), user);
        }

        public async Task<List<GuestUser>> GetPaginatedGuestUserAsync(int page, int pageSize)
        {
            var totalCount = await _guestUsersCollection.CountDocumentsAsync(_ => true); 

            var guestUsers = await _guestUsersCollection
                .Find(_ => true) 
                .Skip((page - 1) * pageSize) 
                .Limit(pageSize) 
                .ToListAsync();

            return guestUsers;
        }


    }
}
