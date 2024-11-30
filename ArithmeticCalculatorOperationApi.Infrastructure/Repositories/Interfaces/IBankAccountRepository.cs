using ArithmeticCalculatorUserApi.Infrastructure.Models;
using MySql.Data.MySqlClient;

namespace ArithmeticCalculatorUserApi.Infrastructure.Repositories
{
    public interface IBankAccountRepository
    {
        Task<IEnumerable<BankAccountEntity>> GetBankAccountsByUserIdAsync(Guid userId);

        Task<bool> AddBalanceAsync(Guid accountId, decimal amount);

        Task<bool> DebitBalanceAsync(Guid accountId, decimal amount);

        Task<bool> AccountBelongsToUserAsync(Guid accountId, Guid userId);

        Task<bool> CreateBankAccountAsync(
            Guid userId,
            decimal initialBalance,
            MySqlConnection connection,
            MySqlTransaction transaction);
    }
}
