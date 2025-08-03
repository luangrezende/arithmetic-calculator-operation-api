using ArithmeticCalculatorOperationApi.Application.Interfaces.Services;
using ArithmeticCalculatorOperationApi.Application.Models.Response;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public class UserService : IUserService
{
    private readonly LambdaInvoker _lambdaInvoker;
    private readonly ILogger<UserService> _logger;

    public UserService(LambdaInvoker lambdaInvoker, ILogger<UserService> logger)
    {
        _lambdaInvoker = lambdaInvoker;
        _logger = logger;
    }

    public async Task<decimal> DebitUserBalanceDirectAsync(Guid accountId, decimal operationCost, string token)
    {
        _logger.LogInformation(
            "DebitUserBalanceDirectAsync started: accountId={AccountId}, operationCost={OperationCost}",
            accountId, operationCost);

        var lambdaArn = Environment.GetEnvironmentVariable("USER_LAMBDA_BASE_ARN")
            ?? throw new InvalidOperationException("Missing USER_LAMBDA_BASE_ARN environment variable.");

        var debitEndpoint = Environment.GetEnvironmentVariable("USER_DEBIT_ENDPOINT")
            ?? throw new InvalidOperationException("Missing USER_DEBIT_ENDPOINT environment variable.");

        var payload = new
        {
            httpMethod = "PUT",
            path = debitEndpoint,
            headers = new
            {
                Authorization = $"Bearer {token}",
                ContentType = "application/json"
            },
            body = JsonSerializer.Serialize(new
            {
                accountId = accountId.ToString(),
                amount = operationCost
            })
        };

        try
        {
            var gatewayResponse = await _lambdaInvoker
                .InvokeLambdaAsync<ApiGatewayInvokeResponse>(lambdaArn, payload);

            if (gatewayResponse == null || gatewayResponse.StatusCode < 200 || gatewayResponse.StatusCode >= 300)
            {
                var errorMessage = gatewayResponse?.Body ?? "Unknown error from Debit API";
                throw new InvalidOperationException(errorMessage);
            }

            return await GetUpdatedBalanceAsync(accountId, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during Lambda invocation to debit endpoint.");
            throw;
        }
    }

    private async Task<decimal> GetUpdatedBalanceAsync(Guid accountId, string token)
    {
        _logger.LogInformation("GetUpdatedBalanceAsync started: accountId={AccountId}", accountId);

        var lambdaArn = Environment.GetEnvironmentVariable("USER_LAMBDA_BASE_ARN")
            ?? throw new InvalidOperationException("Missing USER_LAMBDA_BASE_ARN environment variable.");

        var profileEndpoint = Environment.GetEnvironmentVariable("USER_PROFILE_ENDPOINT")
            ?? throw new InvalidOperationException("Missing USER_PROFILE_ENDPOINT environment variable.");

        var payload = new
        {
            httpMethod = "GET",
            path = profileEndpoint,
            headers = new
            {
                Authorization = $"Bearer {token}",
                ContentType = "application/json"
            }
        };

        try
        {
            var gatewayResponse = await _lambdaInvoker
                .InvokeLambdaAsync<ApiGatewayInvokeResponse>(lambdaArn, payload);

            if (string.IsNullOrEmpty(gatewayResponse?.Body))
                throw new InvalidOperationException("Failed to retrieve user profile.");

            var apiResponse = JsonSerializer.Deserialize<UserApiResponse<UserProfileResponse>>(
                gatewayResponse.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (apiResponse?.Data?.Accounts == null)
                throw new InvalidOperationException("Profile response did not contain accounts.");

            var account = apiResponse.Data.Accounts.FirstOrDefault(a => a.Id == accountId);
            if (account == null)
            {
                _logger.LogError("Account with ID {AccountId} not found in profile response", accountId);
                throw new KeyNotFoundException($"Account with ID {accountId} not found.");
            }

            _logger.LogInformation("Updated balance retrieved: {Balance}", account.Balance);
            return account.Balance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during Lambda invocation to profile endpoint.");
            throw;
        }
    }
}
