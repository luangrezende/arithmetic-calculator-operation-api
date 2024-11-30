using ArithmeticCalculatorOperationApi.Infrastructure.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace ArithmeticCalculatorOperationApi.Infrastructure.Repositories
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly string _connectionString;

        public BankAccountRepository()
        {
            _connectionString = Environment.GetEnvironmentVariable("mysqlConnectionString")
                                ?? throw new InvalidOperationException("Connection string is not set.");
        }

        public async Task<bool> AccountBelongsToUserAsync(Guid accountId, Guid userId)
        {
            const string query = @"
                SELECT COUNT(1)
                FROM bank_account
                WHERE id = @AccountId AND user_id = @UserId";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            return Convert.ToInt32(await ExecuteScalarAsync(query, new Dictionary<string, object>
            {
                { "@AccountId", accountId },
                { "@UserId", userId }
            }, connection)) > 0;
        }

        public async Task<bool> AddBalanceAsync(Guid accountId, decimal amount)
        {
            const string query = @"
                UPDATE bank_account
                SET balance = balance + @Amount,
                    updated_at = CURRENT_TIMESTAMP
                WHERE id = @AccountId";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            return await ExecuteNonQueryAsync(query, new Dictionary<string, object>
            {
                { "@Amount", amount },
                { "@AccountId", accountId }
            }, connection) > 0;
        }

        public async Task<bool> DebitBalanceAsync(Guid accountId, decimal amount)
        {
            const string checkBalanceQuery = @"
                SELECT balance 
                FROM bank_account 
                WHERE id = @AccountId";

            const string debitQuery = @"
                UPDATE bank_account
                SET balance = balance - @Amount,
                    updated_at = CURRENT_TIMESTAMP
                WHERE id = @AccountId";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var balanceObj = await ExecuteScalarAsync(checkBalanceQuery, new Dictionary<string, object>
            {
                { "@AccountId", accountId }
            }, connection);

            if (balanceObj == null || Convert.ToDecimal(balanceObj) < amount)
            {
                return false;
            }

            return await ExecuteNonQueryAsync(debitQuery, new Dictionary<string, object>
            {
                { "@Amount", amount },
                { "@AccountId", accountId }
            }, connection) > 0;
        }

        public async Task<IEnumerable<BankAccountEntity>> GetBankAccountsByUserIdAsync(Guid userId)
        {
            const string query = @"
                SELECT 
                    id, 
                    balance, 
                    currency 
                FROM bank_account 
                WHERE user_id = @UserId";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            var accounts = new List<BankAccountEntity>();

            while (await reader.ReadAsync())
            {
                accounts.Add(new BankAccountEntity
                {
                    Id = reader.GetGuid("id"),
                    Balance = reader.GetDecimal("balance"),
                    Currency = reader.GetString("currency"),
                });
            }

            return accounts;
        }

        public async Task<bool> CreateBankAccountAsync(
            Guid userId,
            decimal initialBalance,
            MySqlConnection connection,
            MySqlTransaction transaction)
        {
            const string query = @"
                INSERT INTO bank_account (id, user_id, balance) 
                VALUES (@Id, @UserId, @Balance)";

            var parameters = new Dictionary<string, object>
            {
                { "@Id", Guid.NewGuid() },
                { "@UserId", userId },
                { "@Balance", initialBalance }
            };

            return await ExecuteNonQueryAsync(query, parameters, connection, transaction) > 0;
        }

        private async Task<int> ExecuteNonQueryAsync(
            string query,
            Dictionary<string, object> parameters,
            MySqlConnection connection,
            MySqlTransaction? transaction = null)
        {
            using var cmd = new MySqlCommand(query, connection, transaction);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            return await cmd.ExecuteNonQueryAsync();
        }

        private async Task<object?> ExecuteScalarAsync(
            string query,
            Dictionary<string, object> parameters,
            MySqlConnection connection)
        {
            using var cmd = new MySqlCommand(query, connection);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            return await cmd.ExecuteScalarAsync();
        }
    }
}
