using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ArithmeticCalculatorUserApi.Infrastructure.Security
{
    public class JwtTokenValidator
    {
        private readonly string _secretKey;

        public JwtTokenValidator(string secretKey)
        {
            _secretKey = secretKey;
        }

        public bool ValidateToken(string token, out Guid userId, bool allowExpired = false)
        {
            userId = Guid.Empty;
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = !allowExpired
            };

            var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken)
            {
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "id");
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
                {
                    userId = parsedUserId;
                    return true;
                }
            }

            return false;
        }

    }
}
