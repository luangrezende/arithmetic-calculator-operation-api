using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArithmeticCalculatorUserApi.Domain.Enums;
using ArithmeticCalculatorUserApi.Domain.Models.DTO;
using ArithmeticCalculatorUserApi.Domain.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace ArithmeticCalculatorUserApi.Infrastructure.Security
{
    public class TokenGeneratorService : ITokenGeneratorService
    {
        private readonly string _jwtSecret;

        public TokenGeneratorService()
        {
            _jwtSecret = Environment.GetEnvironmentVariable("jwtSecretKey")!;
        }

        public string GenerateToken(UserDTO user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("id", user.Id.ToString()),
                new Claim("status", user.Status),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "arithmetic-calculator",
                audience: "arithmetic-calculator",
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds((int)TokenConfiguration.AccessTokenExpirationTimeInSeconds),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
