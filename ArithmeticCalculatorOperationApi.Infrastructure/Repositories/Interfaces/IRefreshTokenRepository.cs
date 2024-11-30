using ArithmeticCalculatorOperationApi.Infrastructure.Models;

namespace ArithmeticCalculatorOperationApi.Infrastructure.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<bool> AddAsync(RefreshTokenEntity refreshToken);

        Task<RefreshTokenEntity?> GetByTokenAsync(string token);

        Task<bool> InvalidateTokenAsync(string token);
    }
}
