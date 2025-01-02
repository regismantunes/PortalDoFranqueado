using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System;
using PortalDoFranqueadoAPI.Models.Entities;

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
                Subject = new ClaimsIdentity(
                [
                    //new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.Integer),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.Name, user.Id.ToString()),
                    new(ClaimTypes.Role, Enum.GetName(user.Role))
                ]),
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
