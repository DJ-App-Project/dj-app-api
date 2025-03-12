using dj_api.Data;
using dj_api.Models;
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
            return await _usersCollection.Find(_ => true).ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

       
    }
}
