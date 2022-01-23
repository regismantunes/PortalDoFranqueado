using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PortalDoFranqueadoAPI.Models;
using Microsoft.IdentityModel.Tokens;

namespace PortalDoFranqueadoAPI.Services
{
    public static class TokenService
    {
        public static AuthenticateData GerarTokenJwt(string secretToken, User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretToken);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    //new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.Integer),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new AuthenticateData()
            {
                Token = tokenHandler.WriteToken(token),
                Expires = tokenDescriptor.Expires.Value
            };
        }
    }
}
