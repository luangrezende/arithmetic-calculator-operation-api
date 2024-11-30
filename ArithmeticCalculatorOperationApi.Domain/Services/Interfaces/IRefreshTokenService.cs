using ArithmeticCalculatorOperationApi.Domain.Models.DTO;

namespace ArithmeticCalculatorOperationApi.Domain.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<string> AddAsync(Guid userId);

        Task<RefreshTokenDTO?> GetByTokenAsync(string token);

        Task<bool> InvalidateTokenAsync(string token);
    }
}
