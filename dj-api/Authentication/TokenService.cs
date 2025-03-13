using dj_api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace dj_api.Authentication
{
    public class TokenService
    {
        private readonly string _secretKey = "f5be22a679e35ba82f04d1427dbe56b8fc7301e529a1322110715467da59e7ce"; //test keys
        public string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name)
           
        };

            var token = new JwtSecurityToken(
                issuer: "test",
                audience: "test2",
                claims: claims,
                expires: DateTime.Now.AddHours(12), 
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
