using ArithmeticCalculatorOperationApi.Infrastructure.Models;
using ArithmeticCalculatorOperationApi.Infrastructure.Repositories.Interfaces;
using MySql.Data.MySqlClient;
using System.Data;

namespace ArithmeticCalculatorOperationApi.Infrastructure.Repositories
{
    public class OperationRepository : IOperationRepository
    {
        private readonly string _connectionString;

        public OperationRepository()
        {
            _connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")
                                ?? throw new InvalidOperationException("Connection string is not set.");
        }

        private async Task<List<OperationRecordEntity>> ExecuteOperationQueryAsync(string sql, MySqlParameter[] parameters)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddRange(parameters);

            using var reader = await cmd.ExecuteReaderAsync();
            var operations = new List<OperationRecordEntity>();

            while (await reader.ReadAsync())
            {
                operations.Add(new OperationRecordEntity
                {
                    Id = reader.GetGuid("id"),
                    UserId = reader.GetGuid("user_id"),
                    Cost = reader.GetDecimal("cost"),
                    UserBalance = reader.GetDecimal("user_balance"),
                    Result = reader.GetString("result"),
                    Expression = reader.GetString("expression"),
                    CreatedAt = reader.GetDateTime("created_at"),
                });
            }

            return operations;
        }

        private string BuildQueryWithFilters(string query)
        {
            return @"
                SELECT 
                    r.id, 
                    r.user_id, 
                    r.cost, 
                    r.user_balance, 
                    r.result, 
                    r.expression, 
                    r.created_at
                FROM operation_record r
                WHERE 
                    r.deleted_at IS NULL AND
                    r.user_id = @UserId AND (
                        @Query = '' OR (
                            LOWER(r.result) LIKE CONCAT('%', LOWER(@Query), '%') OR
                            LOWER(r.expression) LIKE CONCAT('%', LOWER(@Query), '%') OR
                            r.cost LIKE CONCAT('%', @Query, '%') OR
                            (
                                REPLACE(DATE_FORMAT(r.created_at, '%Y/%m/%d'), '/', '') LIKE REPLACE(CONCAT('%', @Query, '%'), '/', '')
                                OR DATE_FORMAT(r.created_at, '%Y-%m-%d %H:%i:%s') LIKE CONCAT('%', @Query, '%')
                            )
                        )
                    )";
        }

        public async Task<bool> SoftDeleteOperationRecordsAsync(Guid userId, List<Guid> recordIds)
        {
            if (recordIds == null || recordIds.Count == 0)
                return false;

            var recordPlaceholders = new List<string>();
            var parameters = new List<MySqlParameter> { new MySqlParameter("@UserId", MySqlDbType.Guid) { Value = userId } };

            int index = 0;
            foreach (var recordId in recordIds)
            {
                var parameterName = $"@RecordId{index}";
                recordPlaceholders.Add(parameterName);
                parameters.Add(new MySqlParameter(parameterName, MySqlDbType.Guid) { Value = recordId });
                index++;
            }

            string sql = $"UPDATE operation_record SET deleted_at = CURRENT_TIMESTAMP WHERE user_id = @UserId AND id IN ({string.Join(", ", recordPlaceholders)}) AND deleted_at IS NULL";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddRange(parameters.ToArray());

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<OperationRecordEntity>> GetPagedOperationsAsync(Guid userId, int page, int pageSize, string query)
        {
            var sql = BuildQueryWithFilters(query);
            sql += " ORDER BY r.created_at DESC LIMIT @PageSize OFFSET @Offset;";

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.Guid) { Value = userId },
                new MySqlParameter("@Query", MySqlDbType.Text) { Value = query },
                new MySqlParameter("@PageSize", MySqlDbType.Int32) { Value = pageSize },
                new MySqlParameter("@Offset", MySqlDbType.Int32) { Value = page * pageSize }
            };

            return await ExecuteOperationQueryAsync(sql, parameters);
        }

        public async Task<int> GetTotalCountAsync(Guid userId, string query)
        {
            var sql = BuildQueryWithFilters(query);
            sql = "SELECT COUNT(*) " + sql.Substring(sql.IndexOf("FROM"));

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.Guid) { Value = userId },
                new MySqlParameter("@Query", MySqlDbType.Text) { Value = query }
            };

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddRange(parameters);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> SaveRecordAsync(OperationRecordEntity operationRecord)
        {
            const string query = @"
                INSERT INTO operation_record (
                    id, 
                    user_id, 
                    cost, 
                    user_balance, 
                    result, 
                    expression, 
                    created_at
                ) VALUES (
                    @Id, 
                    @UserId, 
                    @Cost, 
                    @UserBalance, 
                    @Result, 
                    @Expression, 
                    @CreatedAt
                )";

            var parameters = new[]
            {
                new MySqlParameter("@Id", MySqlDbType.Guid) { Value = operationRecord.Id },
                new MySqlParameter("@UserId", MySqlDbType.Guid) { Value = operationRecord.UserId },
                new MySqlParameter("@Cost", MySqlDbType.Decimal) { Value = operationRecord.Cost },
                new MySqlParameter("@UserBalance", MySqlDbType.Decimal) { Value = operationRecord.UserBalance },
                new MySqlParameter("@Result", MySqlDbType.Text) { Value = operationRecord.Result },
                new MySqlParameter("@Expression", MySqlDbType.Text) { Value = operationRecord.Expression },
                new MySqlParameter("@CreatedAt", MySqlDbType.Timestamp) { Value = operationRecord.CreatedAt }
            };

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddRange(parameters);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}
