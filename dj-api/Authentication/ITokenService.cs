using dj_api.Models;

namespace dj_api.Authentication
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
    }
}