using dj_api.Data;
using dj_api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var users = await _usersCollection.Find(user => true).ToListAsync();

            // Convert ObjectId to String when returning data
            foreach (var user in users)
            {
                user.Id = user.Id.ToString();
            }

            return users;
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            var objectId = ObjectId.Parse(id); // Convert string ID back to ObjectId
            var user = await _usersCollection.Find(u => u.Id == objectId.ToString()).FirstOrDefaultAsync();

            if (user != null)
            {
                user.Id = user.Id.ToString();
            }

            return user;
        }

        public async Task CreateUserAsync(User user)
        {
            await _usersCollection.InsertOneAsync(user);
        }

        public async Task DeleteUserAsync(string id)
        {
            await _usersCollection.DeleteOneAsync(user => user.Id == id);
        }

        public async Task UpdateUserAsync(string id, User user)
        {
            await _usersCollection.ReplaceOneAsync(user => user.Id == id, user);
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _usersCollection.Find(user => user.Email == email).FirstOrDefaultAsync();
        }

    }
}
