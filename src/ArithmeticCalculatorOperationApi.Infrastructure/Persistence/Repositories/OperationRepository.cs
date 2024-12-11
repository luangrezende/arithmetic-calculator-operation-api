using ArithmeticCalculatorOperationApi.Application.Interfaces.Repositories;
using ArithmeticCalculatorOperationApi.Application.Interfaces.Services;
using ArithmeticCalculatorOperationApi.Domain.Entities;
using MySql.Data.MySqlClient;
using System.Data;

namespace ArithmeticCalculatorOperationApi.Infrastructure.Persistence.Repositories
{
    public class OperationRepository : IOperationRepository
    {
        private readonly IDbConnectionService _dbConnectionService;

        public OperationRepository(IDbConnectionService dbConnectionService)
        {
            _dbConnectionService = dbConnectionService;
        }

        private async Task<List<OperationRecordEntity>> ExecuteOperationQueryAsync(string sql, MySqlParameter[] parameters)
        {
            using var connection = await _dbConnectionService.CreateConnectionAsync();
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

            using var connection = await _dbConnectionService.CreateConnectionAsync();
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
            sql = string.Concat("SELECT COUNT(*) ", sql.AsSpan(sql.IndexOf("FROM")));

            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.Guid) { Value = userId },
                new MySqlParameter("@Query", MySqlDbType.Text) { Value = query }
            };

            using var connection = await _dbConnectionService.CreateConnectionAsync();
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

            using var connection = await _dbConnectionService.CreateConnectionAsync();
            using var cmd = new MySqlCommand(query, connection);

            cmd.Parameters.AddRange(parameters);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<DashboardEntity> GetDashboardDataAsync(Guid userId)
        {
            const string query = @"
                SELECT 
                    -- Total number of operations for the user
                    (SELECT COUNT(*) 
                     FROM operation_record 
                     WHERE user_id = @UserId) AS TotalOperations,
 
                    -- Total number of operations in the current month
                    (SELECT COUNT(*) 
                     FROM operation_record 
                     WHERE user_id = @UserId AND deleted_at IS NULL
                     AND MONTH(created_at) = MONTH(CURRENT_DATE)
                     AND YEAR(created_at) = YEAR(CURRENT_DATE)) AS TotalMonthlyOperations,

                    -- Total credit added
                    (SELECT COALESCE(SUM(amount), 0) 
                     FROM balance_record br
                     JOIN bank_account ba ON br.account_id = ba.id
                     WHERE br.type = 'credit' AND ba.user_id = @UserId) AS TotalCredit,

                    -- Total amount of money added in the current year
                    (SELECT COALESCE(SUM(CAST(amount AS DECIMAL(15, 2))), 0) 
                     FROM balance_record br
                     JOIN bank_account ba ON br.account_id = ba.id
                     WHERE ba.user_id = @UserId 
                     AND br.type = 'credit'
                     AND YEAR(br.created_at) = YEAR(CURRENT_DATE)) AS TotalAnnualCashAdded,

                    -- Total number of operations on the platform
                    (SELECT COUNT(*) 
                     FROM operation_record) AS TotalPlatformOperations,

                    -- Total amount of money spent on the platform
                    (SELECT COALESCE(SUM(cost), 0) 
                     FROM operation_record) AS TotalPlatformCashSpent,

                    -- Total amount of money added to the platform
                    (SELECT COALESCE(SUM(amount), 0) 
                     FROM balance_record br
                     JOIN bank_account ba ON br.account_id = ba.id
                     WHERE br.type = 'credit') AS TotalPlatformCashAdded
            ";


            var parameters = new[]
            {
                new MySqlParameter("@UserId", MySqlDbType.Guid) { Value = userId }
            };

            using var connection = await _dbConnectionService.CreateConnectionAsync();
            using var cmd = new MySqlCommand(query, connection);

            cmd.Parameters.AddRange(parameters);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new DashboardEntity
                {
                    TotalOperations = reader.GetInt32("TotalOperations"),
                    TotalMonthlyOperations = reader.GetInt32("TotalMonthlyOperations"),
                    TotalCredit = reader.GetDecimal("TotalCredit"),
                    TotalAnnualCashAdded = reader.GetDecimal("TotalAnnualCashAdded"),
                    TotalPlatformOperations = reader.GetInt32("TotalPlatformOperations"),
                    TotalPlatformCashSpent = reader.GetDecimal("TotalPlatformCashSpent"),
                    TotalPlatformCashAdded = reader.GetDecimal("TotalPlatformCashAdded")
                };
            }

            return new DashboardEntity();
        }
    }
}
