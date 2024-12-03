using ArithmeticCalculatorOperationApi.Domain.Models.Response;
using ArithmeticCalculatorOperationApi.Domain.Services.Interfaces;
using System.Text.Json;

public class UserService : IUserService
{
    private readonly LambdaInvoker _lambdaInvoker;
    private string FunctionName;

    public UserService(LambdaInvoker lambdaInvoker)
    {
        _lambdaInvoker = lambdaInvoker;
        FunctionName = Environment.GetEnvironmentVariable("ARITHMETIC_CALCULATOR_USER_API_FUNCTION_ARN")!;
    }

    public async Task<decimal> DebitUserBalanceDirectAsync(Guid accountId, decimal operationCost, string token)
    {
        var bodyContent = new
        {
            accountId = accountId.ToString(),
            amount = operationCost
        };

        var payload = new
        {
            httpMethod = "PUT",
            path = Environment.GetEnvironmentVariable("USER_DEBIT_API_ENDPOINT"),
            body = JsonSerializer.Serialize(bodyContent),
            headers = new
            {
                Authorization = $"Bearer {token}"
            }
        };

        var debitResponse = await _lambdaInvoker.InvokeLambdaAsync<OuterResponse>(FunctionName, payload);

        if (debitResponse.StatusCode != 200 || debitResponse.Body == null)
            throw new InvalidOperationException($"Failed to process the debit response from the Lambda function.");

        return await GetUpdatedBalanceAsync(accountId, token);
    }

    private async Task<decimal> GetUpdatedBalanceAsync(Guid accountId, string token)
    {
        var profilePayload = new
        {
            httpMethod = "GET",
            path = Environment.GetEnvironmentVariable("USER_PROFILE_API_ENDPOINT"),
            headers = new { Authorization = $"Bearer {token}" }
        };

        var profileResponse = await _lambdaInvoker.InvokeLambdaAsync<OuterResponse>(FunctionName, profilePayload);

        if (profileResponse.StatusCode != 200 || profileResponse.Body == null)
            throw new InvalidOperationException("Failed to process the profile response from the Lambda function.");

        var profileInnerResponse = JsonSerializer.Deserialize<UserApiResponse<UserProfileResponse>>(profileResponse.Body);

        var account = profileInnerResponse?.Data.Accounts?.FirstOrDefault(a => a.Id == accountId);
        return account?.Balance ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");
    }
}
