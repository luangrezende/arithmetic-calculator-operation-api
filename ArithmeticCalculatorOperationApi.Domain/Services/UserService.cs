using Amazon.Lambda;
using Amazon.Lambda.Model;
using ArithmeticCalculatorOperationApi.Domain.Models.Response;
using ArithmeticCalculatorOperationApi.Domain.Services.Interfaces;
using System.Text.Json;

namespace ArithmeticCalculatorOperationApi.Domain.Services
{
    public class UserService : IUserService
    {
        private readonly IAmazonLambda _lambdaClient;

        public UserService(IAmazonLambda lambdaClient)
        {
            _lambdaClient = lambdaClient;
        }

        public async Task<decimal> DebitUserBalanceDirectAsync(Guid accountId, decimal amount, string token)
        {
            var functionName = "ArithmeticCalculatorUserApi";

            var bodyContent = new
            {
                accountId = accountId.ToString(),
                amount = 2.50,
            };

            var payload = new
            {
                httpMethod = "PUT",
                path = "/v1/user/account/balance",
                body = JsonSerializer.Serialize(bodyContent),
                headers = new
                {
                    Authorization = $"Bearer {token}"
                }
            };

            var serializedPayload = JsonSerializer.Serialize(payload);

            var debitRequest = new InvokeRequest
            {
                FunctionName = functionName,
                Payload = serializedPayload,
                InvocationType = InvocationType.RequestResponse
            };

            var debitResponse = await _lambdaClient.InvokeAsync(debitRequest);

            if (debitResponse.StatusCode != 200)
            {
                Console.WriteLine($"Failed to invoke Lambda function {functionName}. Status code: {debitResponse.StatusCode}");
                throw new InvalidOperationException($"Failed to invoke Lambda function {functionName}. Status code: {debitResponse.StatusCode}");
            }

            using var debitReader = new StreamReader(debitResponse.Payload);
            var debitResponseContent = await debitReader.ReadToEndAsync();

            var debitOuterResponse = JsonSerializer.Deserialize<OuterResponse>(debitResponseContent);

            if (debitOuterResponse?.StatusCode != 200 || debitOuterResponse.Body == null)
            {
                throw new InvalidOperationException("Failed to process the debit response from the Lambda function.");
            }

            var updatedBalance = await GetUpdatedBalanceAsync(accountId, token);

            return updatedBalance;
        }

        private async Task<decimal> GetUpdatedBalanceAsync(Guid accountId, string token)
        {
            var functionName = "ArithmeticCalculatorUserApi";

            var payload = new
            {
                httpMethod = "GET",
                path = "/v1/user/profile",
                headers = new
                {
                    Authorization = $"Bearer {token}"
                }
            };

            var serializedPayload = JsonSerializer.Serialize(payload);

            var profileRequest = new InvokeRequest
            {
                FunctionName = functionName,
                Payload = serializedPayload,
                InvocationType = InvocationType.RequestResponse
            };

            var profileResponse = await _lambdaClient.InvokeAsync(profileRequest);

            if (profileResponse.StatusCode != 200)
            {
                throw new InvalidOperationException($"Failed to invoke Lambda function {functionName} for profile. Status code: {profileResponse.StatusCode}");
            }

            using var profileReader = new StreamReader(profileResponse.Payload);
            var profileResponseContent = await profileReader.ReadToEndAsync();

            var profileOuterResponse = JsonSerializer.Deserialize<OuterResponse>(profileResponseContent);

            if (profileOuterResponse?.StatusCode != 200 || profileOuterResponse.Body == null)
            {
                throw new InvalidOperationException("Failed to process the profile response from the Lambda function.");
            }

            var profileInnerResponse = JsonSerializer.Deserialize<UserApiResponse<UserProfileResponse>>(profileOuterResponse.Body);

            var account = profileInnerResponse?.Data.Accounts?.FirstOrDefault(a => a.Id == accountId);
            if (account == null)
            {
                throw new KeyNotFoundException($"Account with ID {accountId} not found.");
            }

            return account.Balance;
        }

        private static string GetEnvironmentVariableOrThrow(string key)
        {
            var value = System.Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Environment variable '{key}' is not set.");
            }
            return value;
        }
    }
}

