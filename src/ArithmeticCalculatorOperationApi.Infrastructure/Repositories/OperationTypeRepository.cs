using ArithmeticCalculatorOperationApi.Infrastructure.Models;
using ArithmeticCalculatorOperationApi.Infrastructure.Repositories.Interfaces;
using MySql.Data.MySqlClient;
using System.Data;

namespace ArithmeticCalculatorOperationApi.Infrastructure.Repositories
{
    public class OperationTypeRepository : IOperationTypeRepository
    {
        private readonly string _connectionString;

        public OperationTypeRepository()
        {
            _connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")
                                ?? throw new InvalidOperationException("Connection string is not set.");
        }

        public async Task<List<OperationTypeEntity>> GetAllAsync()
        {
            const string query = @"
                SELECT ot.id, ot.cost, ot.description, ot.created_at, ot.updated_at
                FROM operation_type ot";

            var operationTypes = new List<OperationTypeEntity>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var operationType = new OperationTypeEntity
                {
                    Id = reader.GetGuid("id"),
                    Cost = reader.GetDecimal("cost"),
                    Description = reader.GetString("description"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    UpdatedAt = reader.GetDateTime("updated_at"),
                };
                operationTypes.Add(operationType);
            }

            return operationTypes;
        }


        public async Task<OperationTypeEntity> GetByIdAsync(Guid id)
        {
            const string query = @"
                SELECT ot.id, ot.cost, ot.description, ot.created_at, ot.updated_at
                FROM operation_type ot
                WHERE ot.id = @OperationTypeId";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.Add(new MySqlParameter("@OperationTypeId", MySqlDbType.Guid)
            {
                Value = id
            });

            using var reader = await cmd.ExecuteReaderAsync();
            var operationType = new OperationTypeEntity();

            while (await reader.ReadAsync())
            {
                operationType = new OperationTypeEntity
                {
                    Id = reader.GetGuid("id"),
                    Cost = reader.GetDecimal("cost"),
                    Description = reader.GetString("description"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    UpdatedAt = reader.GetDateTime("updated_at"),
                };
            }

            return operationType;
        }
    }
}
