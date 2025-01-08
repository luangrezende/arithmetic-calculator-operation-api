using ArithmeticCalculatorOperationApi.Application.Interfaces.Repositories;
using ArithmeticCalculatorOperationApi.Application.Interfaces.Services;
using ArithmeticCalculatorOperationApi.Domain.Entities;
using MySql.Data.MySqlClient;
using System.Data;

namespace ArithmeticCalculatorOperationApi.Infrastructure.Persistence.Repositories
{
    public class OperationTypeRepository : IOperationTypeRepository
    {
        private readonly IDbConnectionService _dbConnectionService;

        public OperationTypeRepository(IDbConnectionService dbConnectionService)
        {
            _dbConnectionService = dbConnectionService;
        }

        public async Task<List<OperationTypeEntity>> GetAllAsync()
        {
            const string query = @"
                SELECT ot.id, ot.operator_code, ot.cost, ot.description, ot.created_at, ot.updated_at
                FROM operation_type ot";

            var operationTypes = new List<OperationTypeEntity>();

            using var connection = await _dbConnectionService.CreateConnectionAsync();
            using var cmd = new MySqlCommand(query, connection);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var operationType = new OperationTypeEntity
                {
                    Id = reader.GetGuid("id"),
                    OperatorCode = reader.GetString("operator_code"),
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
                SELECT ot.id, ot.operator_code, ot.cost, ot.description, ot.created_at, ot.updated_at
                FROM operation_type ot
                WHERE ot.id = @OperationTypeId";

            using var connection = await _dbConnectionService.CreateConnectionAsync();
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
                    OperatorCode = reader.GetString("operator_code"),
                    Cost = reader.GetDecimal("cost"),
                    Description = reader.GetString("description"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    UpdatedAt = reader.GetDateTime("updated_at"),
                };
            }

            return operationType;
        }

        public async Task<List<OperationTypeEntity>> GetByOperatorCodesAsync(string[] operators)
        {
            var query = @"
                SELECT ot.id, ot.operator_code, ot.cost, ot.description, ot.created_at, ot.updated_at
                FROM operation_type ot
                WHERE ot.operator_code IN ({0})";

            var parameters = new List<MySqlParameter>();
            var operatorPlaceholders = string.Join(",", operators.Select((op, index) =>
            {
                var paramName = $"@Operator{index}";
                parameters.Add(new MySqlParameter(paramName, MySqlDbType.VarChar) { Value = op });
                return paramName;
            }));

            query = string.Format(query, operatorPlaceholders);

            var operationTypes = new List<OperationTypeEntity>();

            using var connection = await _dbConnectionService.CreateConnectionAsync();
            using var cmd = new MySqlCommand(query, connection);

            cmd.Parameters.AddRange(parameters.ToArray());

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var operationType = new OperationTypeEntity
                {
                    Id = reader.GetGuid("id"),
                    OperatorCode = reader.GetString("operator_code"),
                    Cost = reader.GetDecimal("cost"),
                    Description = reader.GetString("description"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    UpdatedAt = reader.GetDateTime("updated_at"),
                };
                operationTypes.Add(operationType);
            }

            return operationTypes;
        }
    }
}
