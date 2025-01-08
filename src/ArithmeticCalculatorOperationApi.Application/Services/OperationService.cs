using ArithmeticCalculatorOperationApi.Application.Constants;
using ArithmeticCalculatorOperationApi.Application.DTOs;
using ArithmeticCalculatorOperationApi.Application.Helpers;
using ArithmeticCalculatorOperationApi.Application.Interfaces.Repositories;
using ArithmeticCalculatorOperationApi.Application.Interfaces.Services;
using ArithmeticCalculatorOperationApi.Application.Models.Response;
using ArithmeticCalculatorOperationApi.Domain.Configurations;
using ArithmeticCalculatorOperationApi.Domain.Entities;
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
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException(ApiErrorMessages.InvalidExpression);

        try
        {
            if (expression.Equals("random_string", StringComparison.OrdinalIgnoreCase))
                return await _randomStringService.GenerateRandomStringAsync();

            var preparedExpression = PrepareExpression(expression);

            Expression expressionResult = new(preparedExpression);

            var result = expressionResult.Evaluate()?.ToString();

            if (result is not null && result.IsNumeric())
                return result;

            throw new ArgumentException(ApiErrorMessages.InvalidExpression);
        }
        catch
        {
            throw new ArgumentException(ApiErrorMessages.InvalidExpression);
        }
    }

    public async Task<decimal> CalculateOperationPriceAsync(string expression)
    {
        var operators = ExtractOperators(expression).ToList();

        if (expression.Equals("random_string", StringComparison.OrdinalIgnoreCase))
            operators.Add("random_string");

        if (operators.Count == 0)
            throw new ArgumentException(ApiErrorMessages.InvalidExpression);

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
        if (recordIds == null || recordIds.Count == 0)
            return false;

        return await _operationRepository.SoftDeleteOperationRecordsAsync(userId, recordIds);
    }

    public async Task<DashboardResponse> GetDashboardDataAsync(Guid userId)
    {
        var result = await _operationRepository.GetDashboardDataAsync(userId);

        return new DashboardResponse
        {
            TotalOperations = result.TotalOperations,
            TotalMonthlyOperations = result.TotalMonthlyOperations,
            TotalCredit = result.TotalCredit,
            TotalAnnualCashAdded = result.TotalAnnualCashAdded,
            TotalPlatformOperations = result.TotalPlatformOperations,
            TotalPlatformCashSpent = result.TotalPlatformCashSpent,
            TotalPlatformCashAdded = result.TotalPlatformCashAdded,
            AnnualTarget = Convert.ToDecimal(OperationConfiguration.AnnualOperationCashTarget)
        };
    }
}
