using ArithmeticCalculatorOperationApi.Domain.Models.DTO;
using ArithmeticCalculatorOperationApi.Domain.Services.Interfaces;
using ArithmeticCalculatorOperationApi.Infrastructure.Models;
using ArithmeticCalculatorOperationApi.Infrastructure.Repositories.Interfaces;

namespace ArithmeticCalculatorOperationApi.Domain.Services
{
    public class OperationService : IOperationService
    {
        private readonly IOperationRepository _operationRepository;

        public OperationService(IOperationRepository operationRepository)
        {
            _operationRepository = operationRepository;
        }

        public async Task<(string result, string operationValues)> CalculateOperationResult(string operationType, decimal value1, decimal? value2 = null)
        {
            return await Task.Run(() =>
            {
                var operation = operationType switch
                {
                    "Addition" => $"{value1} + {value2 ?? 0}",
                    "Subtraction" => $"{value1} - {value2 ?? 0}",
                    "Multiplication" => $"{value1} * {value2 ?? 1}",
                    "Division" => value2.HasValue && value2.Value != 0
                        ? $"{value1} / {value2.Value}"
                        : throw new InvalidOperationException("Division by zero is not allowed"),
                    "SquareRoot" => value1 >= 0
                        ? $"√{value1}"
                        : throw new InvalidOperationException("Square root of a negative number is not allowed"),
                    _ => throw new InvalidOperationException("Unknown operation type")
                };

                var result = operationType switch
                {
                    "Addition" => (value1 + (value2 ?? 0)).ToString(),
                    "Subtraction" => (value1 - (value2 ?? 0)).ToString(),
                    "Multiplication" => (value1 * (value2 ?? 1)).ToString(),
                    "Division" => (value1 / value2!.Value).ToString(),
                    "SquareRoot" => Math.Sqrt((double)value1).ToString(),
                    _ => throw new InvalidOperationException("Unknown operation type")
                };

                return (result, operationValues: operation);
            });
        }

        public async Task<bool> SaveOperationRecordAsync(OperationRecordDTO operationRecord)
        {
            return await _operationRepository.SaveOperationRecordAsync(new OperationRecordEntity
            {
                Cost = operationRecord.Cost,
                OperationResult = operationRecord.OperationResult,
                OperationTypeId = operationRecord.OperationTypeId,
                OperationValues = operationRecord.OperationValues,
                UserBalance = operationRecord.UserBalance,
                UserId = operationRecord.UserId,
            });
        }
    }
}
