using System.Data;
using ArithmeticCalculatorOperationApi.Infrastructure.Security;
using ArithmeticCalculatorOperationApi.Infrastructure.Extensions;
using MySql.Data.MySqlClient;
using ArithmeticCalculatorOperationApi.Infrastructure.Models;

namespace ArithmeticCalculatorOperationApi.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        private readonly decimal _promotionalAmount;
        private readonly IBankAccountRepository _bankAccountRepository;

        public UserRepository(IBankAccountRepository bankAccountRepository)
        {
            _bankAccountRepository = bankAccountRepository;
            _connectionString = Environment.GetEnvironmentVariable("mysqlConnectionString") ?? throw new InvalidOperationException("Connection string is not set.");
            _promotionalAmount = decimal.Parse(Environment.GetEnvironmentVariable("promotionalAmount") ?? throw new InvalidOperationException("Promotional amount is not set."));
        }

        public async Task<UserEntity?> AuthenticateAsync(string username, string password)
        {
            const string query = @"
                SELECT u.id, u.username, u.password, u.name, us.description AS status 
                FROM user u
                INNER JOIN user_status us ON u.user_status_id = us.id
                WHERE u.username = @Username";

            var user = await GetUserFromQueryAsync(query, new Dictionary<string, object>
            {
                { "@Username", username }
            });

            if (user == null || !PasswordHasher.VerifyPassword(password, user.Password!))
                return null;

            return user;
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            const string query = "SELECT COUNT(1) FROM user WHERE username = @Username";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var result = await ExecuteScalarAsync(query, new Dictionary<string, object>
            {
                { "@Username", username }
            });

            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> CreateUserAsync(string username, string password, string name)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var userId = Guid.NewGuid();
                var hashedPassword = PasswordHasher.HashPassword(password);

                const string userQuery = @"
                    INSERT INTO user (id, username, password, name, user_status_id) 
                    VALUES (@Id, @Username, @Password, @Name, 1)";

                var userInserted = await ExecuteNonQueryAsync(userQuery, new Dictionary<string, object>
                {
                    { "@Id", userId },
                    { "@Username", username },
                    { "@Password", hashedPassword },
                    { "@Name", name }
                }, connection, transaction);

                if (userInserted <= 0)
                    throw new Exception("Failed to insert user.");

                var accountCreated = await _bankAccountRepository.CreateBankAccountAsync(userId, _promotionalAmount, connection, transaction);
                if (!accountCreated)
                    throw new Exception("Failed to create bank account.");

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<UserEntity?> GetUserByIdAsync(Guid userId)
        {
            const string query = @"
                SELECT u.id, u.username, u.name, us.description AS status 
                FROM user u
                INNER JOIN user_status us ON u.user_status_id = us.id
                WHERE u.id = @UserId";

            return await GetUserFromQueryAsync(query, new Dictionary<string, object>
            {
                { "@UserId", userId }
            });
        }

        public async Task<UserEntity?> GetUserByUsernameAsync(string username)
        {
            const string query = @"
                SELECT u.id, u.username, u.password, u.name, us.description AS status 
                FROM user u
                INNER JOIN user_status us ON u.user_status_id = us.id
                WHERE u.username = @Username";

            return await GetUserFromQueryAsync(query, new Dictionary<string, object>
            {
                { "@Username", username }
            });
        }

        private async Task<UserEntity?> GetUserFromQueryAsync(string query, Dictionary<string, object> parameters)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            using var reader = await cmd.ExecuteReaderAsync();
            if (reader.Read())
            {
                return new UserEntity
                {
                    Id = reader.GetGuid("id"),
                    Username = reader.GetString("username"),
                    Name = reader.GetString("name"),
                    Status = reader.GetString("status"),
                    Password = reader.HasColumn("password") ? reader["password"] as string : null
                };
            }

            return null;
        }

        private async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters, MySqlConnection connection, MySqlTransaction? transaction = null)
        {
            using var cmd = new MySqlCommand(query, connection, transaction);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            return await cmd.ExecuteNonQueryAsync();
        }

        private async Task<object?> ExecuteScalarAsync(string query, Dictionary<string, object> parameters)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            return await cmd.ExecuteScalarAsync();
        }
    }
}
