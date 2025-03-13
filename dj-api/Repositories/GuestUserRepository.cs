using dj_api.Data;
using dj_api.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dj_api.Repositories
{
    public class GuestUserRepository
    {
        private readonly IMongoCollection<GuestUser> _guestUsersCollection;
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(30) // Cache expiration policy
        };

        public GuestUserRepository(MongoDbContext dbContext, IMemoryCache memoryCache)
        {
            _guestUsersCollection = dbContext.GetCollection<GuestUser>("GuestUser");
            _memoryCache = memoryCache;
        }

      
        public async Task<List<GuestUser>> GetAllUsersAsync()
        {
            const string cacheKey = "all_guest_users"; 

         
            if (!_memoryCache.TryGetValue(cacheKey, out List<GuestUser>? cachedUsers))
            {
              
                cachedUsers = await _guestUsersCollection.Find(_ => true).ToListAsync();
                _memoryCache.Set(cacheKey, cachedUsers, _cacheEntryOptions);
            }

            return cachedUsers ?? new List<GuestUser>();
        }

     
        public async Task<GuestUser> GetUserByIdAsync(string id)
        {
            string cacheKey = $"guest_user_{id}"; 

       
            if (!_memoryCache.TryGetValue(cacheKey, out GuestUser? cachedUser))
            {
             
                cachedUser = await _guestUsersCollection.Find(user => user.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
                if (cachedUser != null)
                {
                 
                    _memoryCache.Set(cacheKey, cachedUser, _cacheEntryOptions);
                }
            }

            return cachedUser ?? throw new Exception($"User with ID {id} not found"); 
        }

       
        public async Task CreateUserAsync(GuestUser user)
        {
        
            var existing = await _guestUsersCollection.Find(u => u.Id == user.Id).FirstOrDefaultAsync();
            if (existing != null)
                throw new Exception($"User with ID {user.Id} already exists"); 

            await _guestUsersCollection.InsertOneAsync(user); 

          
            _memoryCache.Remove("all_guest_users");
        }

       
        public async Task DeleteUserAsync(string id)
        {
          
            var existing = await _guestUsersCollection.Find(u => u.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
            if (existing == null)
                throw new Exception($"User with ID {id} does not exist"); 

           
            await _guestUsersCollection.DeleteOneAsync(u => u.Id == Convert.ToInt32(id));

          
            _memoryCache.Remove($"guest_user_{id}");
            _memoryCache.Remove("all_guest_users");
        }

     
        public async Task UpdateUserAsync(string id, GuestUser user)
        {
        
            var existingUser = await _guestUsersCollection.Find(u => u.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
            if (existingUser == null)
                throw new Exception($"User with ID {id} does not exist"); 

          
            await _guestUsersCollection.ReplaceOneAsync(u => u.Id == Convert.ToInt32(id), user);

          
            _memoryCache.Remove($"guest_user_{id}");
            _memoryCache.Remove("all_guest_users");
        }

      
        public async Task<List<GuestUser>> GetPaginatedGuestUserAsync(int page, int pageSize)
        {
            string cacheKey = $"paginated_guest_users_page_{page}_size_{pageSize}"; 

          
            if (!_memoryCache.TryGetValue(cacheKey, out List<GuestUser>? cachedUsers))
            {
               
                cachedUsers = await _guestUsersCollection
                    .Find(_ => true)
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

               
                _memoryCache.Set(cacheKey, cachedUsers, _cacheEntryOptions);
            }

            return cachedUsers ?? new List<GuestUser>(); 
        }
    }
}
