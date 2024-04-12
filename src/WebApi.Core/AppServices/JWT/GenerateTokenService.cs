using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;

namespace WebApi.Core.AppServices.JWT
{
    public interface IGenerateTokenService
    {
        string GenerateToken(string _secretKey, string _issuer, string _audience, int _expirationTime, string username);
    }
    public class GenerateTokenService: IGenerateTokenService
    {
        public string GenerateToken(string _secretKey, string _issuer, string _audience, int _expirationTime, string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username)
            };

            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(_expirationTime),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
