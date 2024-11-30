using ArithmeticCalculatorUserApi.Domain.Enums;
using ArithmeticCalculatorUserApi.Domain.Models.DTO;
using ArithmeticCalculatorUserApi.Domain.Services.Interfaces;
using ArithmeticCalculatorUserApi.Infrastructure.Models;
using ArithmeticCalculatorUserApi.Infrastructure.Repositories;

namespace ArithmeticCalculatorUserApi.Domain.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<string> AddAsync(Guid userId)
        {
            var newRefreshToken = new RefreshTokenEntity
            {
                ExpiresAt = DateTime.UtcNow.AddHours((int)TokenConfiguration.RefreshTokenExpirationTimeInHours),
                UserId = userId
            };

            await _refreshTokenRepository.AddAsync(newRefreshToken);

            return newRefreshToken.Token;
        }

        public async Task<RefreshTokenDTO?> GetByTokenAsync(string token)
        {
            var result = await _refreshTokenRepository.GetByTokenAsync(token);

            return result == null ? null : new RefreshTokenDTO
            {
                ExpiresAt = result.ExpiresAt,
                Token = result.Token,
                CreatedAt = result.CreatedAt,
                IsRevoked = result.IsRevoked,
                IsUsed = result.IsUsed,
                RevokedAt = result.RevokedAt,
                UserId = result.UserId
            };
        }

        public async Task<bool> InvalidateTokenAsync(string token)
        {
            return await _refreshTokenRepository.InvalidateTokenAsync(token);
        }
    }
}
