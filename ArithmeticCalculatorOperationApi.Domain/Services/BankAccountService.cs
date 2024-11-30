using ArithmeticCalculatorUserApi.Domain.Models.DTO;
using ArithmeticCalculatorUserApi.Domain.Services.Interfaces;
using ArithmeticCalculatorUserApi.Infrastructure.Repositories;

namespace ArithmeticCalculatorUserApi.Domain.Services
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IBankAccountRepository _bankAccountRepository;

        public BankAccountService(IBankAccountRepository bankAccountRepository)
        {
            _bankAccountRepository = bankAccountRepository;
        }

        public async Task<bool> AccountBelongsToUserAsync(Guid accountId, Guid userId)
        {
            return await _bankAccountRepository.AccountBelongsToUserAsync(accountId, userId);
        }

        public async Task<bool> AddBalanceAsync(Guid accountId, decimal amount)
        {
            return await _bankAccountRepository.AddBalanceAsync(accountId, amount);
        }

        public async Task<bool> DebitBalanceAsync(Guid accountId, decimal amount)
        {
            return await _bankAccountRepository.DebitBalanceAsync(accountId, amount);
        }

        public async Task<List<BankAccountDTO>?> GetBankAccountsByUserIdAsync(Guid userId)
        {
            var bankAccounts = await _bankAccountRepository.GetBankAccountsByUserIdAsync(userId);

            return bankAccounts?.Select(account => new BankAccountDTO
            {
                Id = account.Id,
                UserId = account.UserId,
                Balance = account.Balance,
                CreatedAt = account.CreatedAt,
                Currency = account.Currency,
                UpdatedAt = account.UpdatedAt
            }).ToList();
        }
    }
}
