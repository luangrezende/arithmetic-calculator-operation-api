using ArithmeticCalculatorOperationApi.Infrastructure.Models;
using ArithmeticCalculatorOperationApi.Infrastructure.Repositories.Interfaces;
using MySql.Data.MySqlClient;

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
