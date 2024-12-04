using ArithmeticCalculatorOperationApi.Domain.Constants;
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
        if (string.IsNullOrWhiteSpace(operationType))
            throw new ArgumentException(OperationsMessages.UnknownOperationTypeError);

        var normalizedOperationType = operationType.Replace(" ", "").ToLower();

        return normalizedOperationType switch
        {
            "randomstring" => await GenerateRandomStringResultAsync(),
            "addition" => CalculateArithmeticOperation(value1, value2 ?? 0, "+", (a, b) => a + b),
            "subtraction" => CalculateArithmeticOperation(value1, value2 ?? 0, "-", (a, b) => a - b),
            "multiplication" => CalculateArithmeticOperation(value1, value2 ?? 1, "*", (a, b) => a * b),
            "division" => value2.HasValue && value2.Value != 0
                ? CalculateArithmeticOperation(value1, value2.Value, "/", (a, b) => a / b)
                : throw new InvalidOperationException(OperationsMessages.DivisionByZeroError),
            "squareroot" => value1 >= 0
                ? (Math.Sqrt((double)value1).ToString(), $"√{value1}")
                : throw new InvalidOperationException(OperationsMessages.NegativeSquareRootError),
            _ => throw new InvalidOperationException(OperationsMessages.UnknownOperationTypeError)
        };
    }

    private async Task<(string result, string operationValues)> GenerateRandomStringResultAsync()
    {
        var randomString = await _randomStringService.GenerateRandomStringAsync();
        return (randomString, OperationsMessages.RandomStringDescription);
    }

    private (string result, string operationValues) CalculateArithmeticOperation(decimal value1, decimal value2, string operatorSymbol, Func<decimal, decimal, decimal> operation)
    {
        var result = operation(value1, value2).ToString();
        var operationValues = $"{value1} {operatorSymbol} {value2}";
        return (result, operationValues);
    }

    public async Task<(int totalRecords, List<OperationRecordDTO> records)> GetPagedOperationsAsync(Guid userId, int page, int pageSize, string query)
    {
        var totalRecords = await _operationRepository.GetTotalCountAsync(userId, query);
        var operations = await _operationRepository.GetPagedOperationsAsync(userId, page, pageSize, query);

        var operationDTOs = operations.Select(ToDTO).ToList();
        return (totalRecords, operationDTOs);
    }

    private static OperationRecordDTO ToDTO(OperationRecordEntity entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        OperationTypeId = entity.OperationTypeId,
        OperationTypeDescription = entity.OperationTypeDescription,
        Cost = entity.Cost,
        UserBalance = entity.UserBalance,
        OperationValues = entity.OperationValues,
        OperationResult = entity.OperationResult,
        CreatedAt = entity.CreatedAt,
    };

    public async Task<OperationRecordDTO?> SaveOperationRecordAsync(OperationRecordDTO operationRecord)
    {
        var retryPolicy = CreateRetryPolicy();

        return await retryPolicy.ExecuteAsync(async () =>
        {
            var operationRecordEntity = ToEntity(operationRecord);

            if (await _operationRepository.SaveRecordAsync(operationRecordEntity))
                return ToDTO(operationRecordEntity);

            return null;
        });
    }

    private static OperationRecordEntity ToEntity(OperationRecordDTO dto) => new()
    {
        Id = Guid.NewGuid(),
        Cost = dto.Cost,
        OperationResult = dto.OperationResult,
        OperationTypeId = dto.OperationTypeId,
        OperationValues = dto.OperationValues,
        UserBalance = dto.UserBalance,
        UserId = dto.UserId,
        CreatedAt = DateTime.UtcNow,
    };

    private static IAsyncPolicy CreateRetryPolicy() =>
        Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Attempt {retryCount} failed. Retrying in {timeSpan.TotalSeconds} seconds...");
                    Console.WriteLine($"Error: {exception.Message}");
                });

    public async Task<bool> SoftDeleteOperationRecordsAsync(Guid userId, List<Guid> recordIds)
    {
        if (recordIds == null || !recordIds.Any())
            return false;

        return await _operationRepository.SoftDeleteOperationRecordsAsync(userId, recordIds);
    }
}
