using dj_api.Data;
using dj_api.Models;
using MongoDB.Driver;

namespace dj_api.Repositories
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UserRepository(MongoDbContext dbContext)
        {
            _usersCollection = dbContext.GetCollection<User>("User");
        }
      
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _usersCollection.Find(_ => true).ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _usersCollection.Find(user => user.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(User NewUser)
        {
            var existing = await _usersCollection.Find(user => user.Id == NewUser.Id).FirstOrDefaultAsync(); // preveri, če uporabnik že obstaja
            if (existing != null)
                throw new Exception($"User s {NewUser.Id} že obstaja"); // če uporabnik že obstaja, vrni Exception

            if (_usersCollection.Find(user => user.Username == NewUser.Username || user.Email == NewUser.Email).Any()) 
                throw new Exception($"Username ali email že uporabljen"); // če uporabnik z emailom ali usernamom že obstaja, vrni Exception

            await _usersCollection.InsertOneAsync(NewUser); // ustvari novega uporabnika
        }

        public async Task DeleteUserAsync(string id)
        {
            var existing = await _usersCollection.Find(user => user.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
            if (existing == null)
                throw new Exception($"User s {id} ne obstaja"); // če uporabnik ne obstaja, vrni Exception

            await _usersCollection.DeleteOneAsync(user => user.Id == Convert.ToInt32(id)); // izbriši uporabnika
        }

        public async Task UpdateUserAsync(string id, User user)
        {
            await _usersCollection.ReplaceOneAsync(user => user.Id == Convert.ToInt32(id), user);
        }
    }
}
