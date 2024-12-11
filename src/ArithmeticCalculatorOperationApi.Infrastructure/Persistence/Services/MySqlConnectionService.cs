using ArithmeticCalculatorOperationApi.Application.Interfaces.Services;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data;

namespace ArithmeticCalculatorOperationApi.Infrastructure.Persistence.Services
{
    public class MySqlConnectionService : IDbConnectionService
    {
        private readonly string _connectionString;

        public MySqlConnectionService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<MySqlConnection> CreateConnectionAsync()
        {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<int> ExecuteNonQueryAsync(
            string query,
            Dictionary<string, object> parameters,
            MySqlConnection connection,
            MySqlTransaction? transaction = null)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using var cmd = new MySqlCommand(query, connection, transaction);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }

            return await cmd.ExecuteNonQueryAsync();
        }


        public async Task<T?> ExecuteScalarAsync<T>(
            string query,
            Dictionary<string, object> parameters,
            MySqlConnection connection,
            MySqlTransaction? transaction = null)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using var cmd = new MySqlCommand(query, connection, transaction);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }

            var result = await cmd.ExecuteScalarAsync();

            return result != null && result != DBNull.Value ? (T)Convert.ChangeType(result, typeof(T)) : default;
        }

        public async Task<DbDataReader> ExecuteReaderAsync(
            string query,
            Dictionary<string, object> parameters,
            MySqlConnection connection)
        {
            var cmd = new MySqlCommand(query, connection);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }
    }
}
