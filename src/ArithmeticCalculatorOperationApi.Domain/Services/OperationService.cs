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
        var normalizedOperationType = operationType?.Replace(" ", "").ToLower();

        if (normalizedOperationType == "randomstring")
        {
            var randomString = await _randomStringService.GenerateRandomStringAsync();
            return (randomString, OperationsMessages.RandomStringDescription);
        }

        return await Task.Run(() =>
        {
            var operation = normalizedOperationType switch
            {
                "addition" => $"{value1} + {value2 ?? 0}",
                "subtraction" => $"{value1} - {value2 ?? 0}",
                "multiplication" => $"{value1} * {value2 ?? 1}",
                "division" => value2.HasValue && value2.Value != 0
                    ? $"{value1} / {value2.Value}"
                    : throw new InvalidOperationException(OperationsMessages.DivisionByZeroError),
                "squareroot" => value1 >= 0
                    ? $"√{value1}"
                    : throw new InvalidOperationException(OperationsMessages.NegativeSquareRootError),
                _ => throw new InvalidOperationException(OperationsMessages.UnknownOperationTypeError)
            };

            var result = normalizedOperationType switch
            {
                "addition" => (value1 + (value2 ?? 0)).ToString(),
                "subtraction" => (value1 - (value2 ?? 0)).ToString(),
                "multiplication" => (value1 * (value2 ?? 1)).ToString(),
                "division" => (value1 / value2!.Value).ToString(),
                "squareroot" => Math.Sqrt((double)value1).ToString(),
                _ => throw new InvalidOperationException(OperationsMessages.UnknownOperationTypeError)
            };

            return (result, operationValues: operation);
        });
    }

    public async Task<(int totalRecords, List<OperationRecordDTO> records)> GetPagedOperationsAsync(Guid userId, int page, int pageSize, string query)
    {
        var totalRecords = await _operationRepository.GetTotalCountAsync(userId, query);
        var operations = await _operationRepository.GetPagedOperationsAsync(userId, page, pageSize, query);

        var operationDTOs = operations.Select(op => new OperationRecordDTO
        {
            Id = op.Id,
            UserId = op.UserId,
            OperationTypeId = op.OperationTypeId,
            OperationTypeDescription = op.OperationTypeDescription,
            Cost = op.Cost,
            UserBalance = op.UserBalance,
            OperationValues = op.OperationValues,
            OperationResult = op.OperationResult,
            CreatedAt = op.CreatedAt,
        }).ToList();

        return (totalRecords, operationDTOs);
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

    public async Task<bool> SoftDeleteOperationRecordsAsync(Guid userId, List<Guid> recordIds)
    {
        return await _operationRepository.SoftDeleteOperationRecordsAsync(userId, recordIds);
    }
}
