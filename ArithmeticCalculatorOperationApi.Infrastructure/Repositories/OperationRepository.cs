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
            _connectionString = Environment.GetEnvironmentVariable("mysqlConnectionString")
                                ?? throw new InvalidOperationException("Connection string is not set.");
        }

        public async Task<bool> SoftDeleteOperationRecordsAsync(Guid userId, List<Guid> recordIds)
        {
            const string sql = @"
                UPDATE record
                SET deleted_at = CURRENT_TIMESTAMP
                WHERE user_id = @UserId AND id IN (@RecordIds) AND deleted_at IS NULL";

            var idsParameter = string.Join(",", recordIds.Select(id => $"'{id}'"));

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(sql.Replace("@RecordIds", idsParameter), connection);
            cmd.Parameters.Add(new MySqlParameter("@UserId", MySqlDbType.Guid) { Value = userId });

            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }

        public async Task<List<OperationRecordEntity>> GetPagedOperationsAsync(Guid userId, int page, int pageSize, string query)
        {
            const string sql = @"
                SELECT 
                    r.id, 
                    r.operation_type_id, 
                    r.user_id, 
                    r.cost, 
                    r.user_balance, 
                    r.operation_result, 
                    r.operation_values, 
                    r.created_at,
                    ot.description AS operation_type_description
                FROM record r
                INNER JOIN operation_type ot ON r.operation_type_id = ot.id
                WHERE 
                    r.user_id = @UserId AND (
                        @Query = '' OR (
                            LOWER(ot.description) LIKE CONCAT('%', LOWER(@Query), '%') OR
                            r.cost LIKE CONCAT('%', @Query, '%') OR
                            LOWER(r.operation_result) LIKE CONCAT('%', LOWER(@Query), '%') OR
                            LOWER(r.operation_values) LIKE CONCAT('%', LOWER(@Query), '%') OR
                            DATE_FORMAT(r.created_at, '%Y-%m-%d %H:%i:%s') LIKE CONCAT('%', @Query, '%')
                        )
                    )
                ORDER BY r.created_at DESC
                LIMIT @PageSize OFFSET @Offset";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.Add(new MySqlParameter("@UserId", MySqlDbType.Guid) { Value = userId });
            cmd.Parameters.Add(new MySqlParameter("@Query", MySqlDbType.Text) { Value = query });
            cmd.Parameters.Add(new MySqlParameter("@PageSize", MySqlDbType.Int32) { Value = pageSize });
            cmd.Parameters.Add(new MySqlParameter("@Offset", MySqlDbType.Int32) { Value = page * pageSize });

            using var reader = await cmd.ExecuteReaderAsync();

            var operations = new List<OperationRecordEntity>();
            while (await reader.ReadAsync())
            {
                operations.Add(new OperationRecordEntity
                {
                    Id = reader.GetGuid("id"),
                    OperationTypeId = reader.GetGuid("operation_type_id"),
                    UserId = reader.GetGuid("user_id"),
                    Cost = reader.GetDecimal("cost"),
                    UserBalance = reader.GetDecimal("user_balance"),
                    OperationResult = reader.GetString("operation_result"),
                    OperationValues = reader.GetString("operation_values"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    OperationTypeDescription = reader.GetString("operation_type_description"),
                });
            }

            return operations;
        }

        public async Task<int> GetTotalCountAsync(Guid userId, string query)
        {
            const string sql = @"
                SELECT COUNT(*)
                    FROM record r
                    INNER JOIN operation_type ot ON r.operation_type_id = ot.id
                    WHERE 
                        r.user_id = @UserId AND (
                            @Query = '' OR (
                                LOWER(ot.description) LIKE CONCAT('%', LOWER(@Query), '%') OR
                                r.cost LIKE CONCAT('%', @Query, '%') OR
                                LOWER(r.operation_result) LIKE CONCAT('%', LOWER(@Query), '%') OR
                                LOWER(r.operation_values) LIKE CONCAT('%', LOWER(@Query), '%') OR
                                DATE_FORMAT(r.created_at, '%Y-%m-%d %H:%i:%s') LIKE CONCAT('%', @Query, '%')
                            )
                        );
                    ";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.Add(new MySqlParameter("@UserId", MySqlDbType.Guid) { Value = userId });
            cmd.Parameters.Add(new MySqlParameter("@Query", MySqlDbType.Text) { Value = query });

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> SaveRecordAsync(OperationRecordEntity operationRecord)
        {
            var operationId = Guid.NewGuid();

            const string query = @"
                INSERT INTO record (
                    id, 
                    operation_type_id, 
                    user_id, 
                    cost, 
                    user_balance, 
                    operation_result, 
                    operation_values, 
                    created_at
                ) VALUES (
                    @Id, 
                    @OperationTypeId, 
                    @UserId, 
                    @Cost, 
                    @UserBalance, 
                    @OperationResult, 
                    @OperationValues, 
                    @CreatedAt
                )";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.Guid) { Value = operationId });
            cmd.Parameters.Add(new MySqlParameter("@OperationTypeId", MySqlDbType.Guid) { Value = operationRecord.OperationTypeId });
            cmd.Parameters.Add(new MySqlParameter("@UserId", MySqlDbType.Guid) { Value = operationRecord.UserId });
            cmd.Parameters.Add(new MySqlParameter("@Cost", MySqlDbType.Decimal) { Value = operationRecord.Cost });
            cmd.Parameters.Add(new MySqlParameter("@UserBalance", MySqlDbType.Decimal) { Value = operationRecord.UserBalance });
            cmd.Parameters.Add(new MySqlParameter("@OperationResult", MySqlDbType.Text) { Value = operationRecord.OperationResult });
            cmd.Parameters.Add(new MySqlParameter("@OperationValues", MySqlDbType.Text) { Value = operationRecord.OperationValues });
            cmd.Parameters.Add(new MySqlParameter("@CreatedAt", MySqlDbType.Timestamp) { Value = operationRecord.CreatedAt });

            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }
    }
}
