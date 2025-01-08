using MySql.Data.MySqlClient;
using System.Data.Common;

namespace ArithmeticCalculatorOperationApi.Application.Interfaces.Services
{
    public interface IDbConnectionService
    {
        /// <summary>
        /// Executes a non-query command (e.g., INSERT, UPDATE, DELETE) asynchronously.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">A dictionary of query parameters.</param>
        /// <param name="connection">The MySQL connection to use.</param>
        /// <param name="transaction">Optional transaction for the query.</param>
        /// <returns>The number of rows affected by the query.</returns>
        Task<int> ExecuteNonQueryAsync(
            string query,
            Dictionary<string, object> parameters,
            MySqlConnection connection,
            MySqlTransaction? transaction = null);

        /// <summary>
        /// Executes a scalar query (e.g., SELECT COUNT(*) or SUM(column)) asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the scalar result.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">A dictionary of query parameters.</param>
        /// <param name="connection">The MySQL connection to use.</param>
        /// <param name="transaction">Optional transaction for the query.</param>
        /// <returns>The result of the scalar query, or null if no result.</returns>
        Task<T?> ExecuteScalarAsync<T>(
            string query,
            Dictionary<string, object> parameters,
            MySqlConnection connection,
            MySqlTransaction? transaction = null);

        /// <summary>
        /// Creates and opens a new MySQL database connection asynchronously.
        /// </summary>
        /// <returns>An open <see cref="MySqlConnection"/> instance.</returns>
        Task<MySqlConnection> CreateConnectionAsync();

        Task<DbDataReader> ExecuteReaderAsync(
            string query,
            Dictionary<string, object> parameters,
            MySqlConnection connection);
    }
}
