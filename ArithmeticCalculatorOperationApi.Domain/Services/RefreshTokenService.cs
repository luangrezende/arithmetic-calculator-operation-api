using ArithmeticCalculatorOperationApi.Domain.Enums;
using ArithmeticCalculatorOperationApi.Domain.Models.DTO;
using ArithmeticCalculatorOperationApi.Domain.Services.Interfaces;
using ArithmeticCalculatorOperationApi.Infrastructure.Models;
using ArithmeticCalculatorOperationApi.Infrastructure.Repositories;

namespace ArithmeticCalculatorOperationApi.Domain.Services
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
