using ArithmeticCalculatorOperationApi.Domain.Models.Response;
using ArithmeticCalculatorOperationApi.Domain.Services.Interfaces;
using System.Text.Json;

public class UserService : IUserService
{
    private readonly LambdaInvoker _lambdaInvoker;
    private const string FunctionName = "ArithmeticCalculatorUserApi";

    public UserService(LambdaInvoker lambdaInvoker)
    {
        _lambdaInvoker = lambdaInvoker;
    }

    public async Task<decimal> DebitUserBalanceDirectAsync(Guid accountId, decimal amount, string token)
    {
        var debitPayload = new
        {
            httpMethod = "PUT",
            path = "/v1/user/account/balance",
            body = new
            {
                accountId = accountId.ToString(),
                amount
            },
            headers = new { Authorization = $"Bearer {token}" }
        };

        var debitResponse = await _lambdaInvoker.InvokeLambdaAsync<OuterResponse>(FunctionName, debitPayload);

        if (debitResponse.StatusCode != 200 || debitResponse.Body == null)
        {
            var test = JsonSerializer.Serialize(debitResponse);

            throw new InvalidOperationException($"Failed to process the debit response from the Lambda function. RESPONSE: {test}");
        }

        return await GetUpdatedBalanceAsync(accountId, token);
    }

    private async Task<decimal> GetUpdatedBalanceAsync(Guid accountId, string token)
    {
        var profilePayload = new
        {
            httpMethod = "GET",
            path = "/v1/user/profile",
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
