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
        public async Task<List<User>> GetPaginatedUserAsync(int page, int pageSize)
        {
            return await _usersCollection
                .Find(_ => true)  
                .Skip((page - 1) * pageSize) 
                .Limit(pageSize) 
                .ToListAsync();
        }

    }
}
