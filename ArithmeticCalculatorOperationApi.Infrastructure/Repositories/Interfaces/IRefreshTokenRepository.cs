using ArithmeticCalculatorUserApi.Infrastructure.Models;

namespace ArithmeticCalculatorUserApi.Infrastructure.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<bool> AddAsync(RefreshTokenEntity refreshToken);

        Task<RefreshTokenEntity?> GetByTokenAsync(string token);

        Task<bool> InvalidateTokenAsync(string token);
    }
}
