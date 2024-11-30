using ArithmeticCalculatorOperationApi.Domain.Models.DTO;

namespace ArithmeticCalculatorOperationApi.Domain.Services.Interfaces
{
    public interface IBankAccountService
    {
        Task<List<BankAccountDTO>?> GetBankAccountsByUserIdAsync(Guid userId);

        Task<bool> AddBalanceAsync(Guid accountId, decimal amount);

        Task<bool> DebitBalanceAsync(Guid accountId, decimal amount);

        Task<bool> AccountBelongsToUserAsync(Guid accountId, Guid userId);
    }
}
