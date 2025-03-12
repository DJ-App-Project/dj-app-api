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
            return await _usersCollection.Find(user => user.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(User user)
        {
            await _usersCollection.InsertOneAsync(user);
        }

        public async Task DeleteUserAsync(string id)
        {
            await _usersCollection.DeleteOneAsync(user => user.Id == Convert.ToInt32(id));
        }

        public async Task UpdateUserAsync(string id, User user)
        {
            await _usersCollection.ReplaceOneAsync(user => user.Id == Convert.ToInt32(id), user);
        }
        public async Task<User?> Authenticate(string username, string password)
        { 
            User? user = await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }
            //bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password); later when we have BCrypt
            if (user.Password != password)
            {
                return null; 
            }
            return user;
        }
        public async Task<User> FindUserByUsername(string username)
        {
            return await _usersCollection.Find(user => user.Username == username ).FirstOrDefaultAsync();
        }
        public async Task<User> FindUserByEmail( string email)
        {
            return await _usersCollection.Find(user =>   user.Email == email).FirstOrDefaultAsync();
        }

    }
}
