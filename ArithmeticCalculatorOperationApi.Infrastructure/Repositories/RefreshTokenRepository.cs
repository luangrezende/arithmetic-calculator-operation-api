using ArithmeticCalculatorOperationApi.Infrastructure.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace ArithmeticCalculatorOperationApi.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly string _connectionString;

        public RefreshTokenRepository()
        {
            _connectionString = Environment.GetEnvironmentVariable("mysqlConnectionString")
                                ?? throw new InvalidOperationException("Connection string is not set.");
        }

        public async Task<bool> AddAsync(RefreshTokenEntity refreshToken)
        {
            const string query = @"
                INSERT INTO refresh_tokens (token, user_id, expires_at, created_at, is_revoked)
                VALUES (@Token, @UserId, @ExpiresAt, @CreatedAt, @IsRevoked)";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Token", refreshToken.Token);
            cmd.Parameters.AddWithValue("@UserId", refreshToken.UserId);
            cmd.Parameters.AddWithValue("@ExpiresAt", refreshToken.ExpiresAt);
            cmd.Parameters.AddWithValue("@CreatedAt", refreshToken.CreatedAt);
            cmd.Parameters.AddWithValue("@IsRevoked", refreshToken.IsRevoked);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<RefreshTokenEntity?> GetByTokenAsync(string token)
        {
            const string query = @"
                SELECT token, user_id, expires_at, created_at, is_revoked 
                FROM refresh_tokens
                WHERE token = @Token AND is_revoked = 0";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Token", token);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RefreshTokenEntity
                {
                    Token = reader.GetString("token"),
                    UserId = reader.GetGuid("user_id"),
                    ExpiresAt = reader.GetDateTime("expires_at"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    IsRevoked = reader.GetBoolean("is_revoked")
                };
            }

            return null;
        }

        public async Task<bool> InvalidateTokenAsync(string token)
        {
            const string query = @"
                UPDATE refresh_tokens
                SET is_revoked = 1, 
                    revoked_at = @RevokedAt
                WHERE token = @Token";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@RevokedAt", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@Token", token);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}
