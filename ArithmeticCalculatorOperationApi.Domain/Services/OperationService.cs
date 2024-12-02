using ArithmeticCalculatorOperationApi.Domain.Models.DTO;
using ArithmeticCalculatorOperationApi.Domain.Services.Interfaces;
using ArithmeticCalculatorOperationApi.Infrastructure.Models;
using ArithmeticCalculatorOperationApi.Infrastructure.Repositories.Interfaces;
using Polly;

public class OperationService : IOperationService
{
    private readonly IOperationRepository _operationRepository;
    private readonly IRandomStringService _randomStringService;

    public OperationService(IOperationRepository operationRepository, IRandomStringService randomStringService)
    {
        _operationRepository = operationRepository;
        _randomStringService = randomStringService;
    }

    public async Task<(string result, string operationValues)> CalculateOperationResult(string operationType, decimal value1, decimal? value2 = null)
    {
        if (operationType == "Random String")
        {
            var randomString = await _randomStringService.GenerateRandomStringAsync();
            return (randomString, "Random string");
        }

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
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Attempt {retryCount} failed. Retrying in {timeSpan.TotalSeconds} seconds...");
                    Console.WriteLine($"Error: {exception.Message}");
                });

        return await retryPolicy.ExecuteAsync(async () =>
        {
            return await _operationRepository.SaveRecordAsync(new OperationRecordEntity
            {
                Cost = operationRecord.Cost,
                OperationResult = operationRecord.OperationResult,
                OperationTypeId = operationRecord.OperationTypeId,
                OperationValues = operationRecord.OperationValues,
                UserBalance = operationRecord.UserBalance,
                UserId = operationRecord.UserId,
            });
        });
    }
}
