using dj_api.Models;

namespace dj_api.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(string id);
        Task<User?> Authenticate(string username, string password);
        Task<User?> FindUserByEmail(string email);
        Task<User?> FindUserByUsername(string username);
        Task CreateUserAsync(User user);
        Task DeleteUserAsync(string id);
        Task UpdateUserAsync(string id, User user);
        Task<List<User>> GetPaginatedUserAsync(int page, int pageSize);
        Task<int> GetTotalActiveUsersAsync();
    }
}