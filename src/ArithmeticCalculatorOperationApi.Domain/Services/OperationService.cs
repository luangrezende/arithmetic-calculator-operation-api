using ArithmeticCalculatorOperationApi.Domain.Models.DTO;
using ArithmeticCalculatorOperationApi.Domain.Services.Interfaces;
using ArithmeticCalculatorOperationApi.Infrastructure.Models;
using ArithmeticCalculatorOperationApi.Infrastructure.Repositories.Interfaces;
using NCalc;
using Polly;
using System.Text.RegularExpressions;

public class OperationService : IOperationService
{
    private readonly IOperationRepository _operationRepository;
    private readonly IOperationTypeRepository _operationTypeRepository;
    private readonly IRandomStringService _randomStringService;

    public OperationService(
        IOperationRepository operationRepository,
        IOperationTypeRepository operationTypeRepository, 
        IRandomStringService randomStringService)
    {
        _operationRepository = operationRepository;
        _operationTypeRepository = operationTypeRepository;
        _randomStringService = randomStringService;
    }

    public async Task<string> CalculateOperation(string expression)
    {
        if (expression.Equals("random_string"))
            return await _randomStringService.GenerateRandomStringAsync();

        var preparedExpression = PrepareExpression(expression);

        Expression expressionResult = new(preparedExpression);

        return expressionResult.Evaluate().ToString()!;
    }

    public async Task<decimal> CalculateOperationPriceAsync(string expression)
    {
        var operators = ExtractOperators(expression).ToList();

        if (expression.Equals("random_string", StringComparison.OrdinalIgnoreCase))
            operators.Add("random_string");

        var operationTypes = await _operationTypeRepository.GetByOperatorCodesAsync([.. operators]);

        decimal totalCost = operationTypes.Sum(op => op.Cost);

        return totalCost;
    }

    private static string PrepareExpression(string expression)
    {
        expression = expression.Replace("√", "Sqrt");
        expression = Regex.Replace(expression, @"(\d+(\.\d+)?)\s*\^\s*(\d+(\.\d+)?)", "Pow($1, $3)");
        expression = Regex.Replace(expression, @"\^\((\d+(\.\d+)?),\s*(\d+(\.\d+)?)\)", "Pow($1, $3)");

        return expression;
    }

    public static string[] ExtractOperators(string expression)
    {
        var operatorPattern = @"[+\-*/^√]";
        var matches = Regex.Matches(expression, operatorPattern);

        var operatorMap = new Dictionary<string, string>
        {
            { "+", "addition" },
            { "-", "subtraction" },
            { "*", "multiplication" },
            { "/", "division" },
            { "^", "exponentiation" },
            { "√", "square_root" }
        };

        HashSet<string> operators = [];

        foreach (Match match in matches)
        {
            var symbol = match.Value;
            if (operatorMap.ContainsKey(symbol))
            {
                operators.Add(operatorMap[symbol]);
            }
        }

        return [.. operators];
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
        Cost = entity.Cost,
        UserBalance = entity.UserBalance,
        Expression = entity.Expression,
        Result = entity.Result,
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
        Result = dto.Result,
        Expression = dto.Expression,
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
