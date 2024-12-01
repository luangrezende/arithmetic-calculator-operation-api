using ArithmeticCalculatorOperationApi.Domain.Models.Request;
using ArithmeticCalculatorOperationApi.Domain.Models.Response;
using ArithmeticCalculatorOperationApi.Domain.Services.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ArithmeticCalculatorOperationApi.Domain.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> DebitUserBalanceAsync(Guid accountId, decimal amount, string token)
        {
            var requestUrl = GetEnvironmentVariableOrThrow("UserDebitApiEndpoint");
            var requestBody = new UpdateBalanceRequest 
            { 
                Amount = amount,
                AccountId = accountId 
            };

            return await SendRequestAsync(HttpMethod.Put, requestUrl, token, requestBody);
        }

        public async Task<decimal> GetUserBalanceAsync(Guid accountId, string token)
        {
            var requestUrl = GetEnvironmentVariableOrThrow("UserProfileApiEndpoint");

            var response = await SendRequestAsync(HttpMethod.Get, requestUrl, token);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to fetch user profile. Status code: {response.StatusCode}");
            }

            var userProfileResponse = await response.Content.ReadFromJsonAsync<UserApiResponse<UserProfileResponse>>();
            if (userProfileResponse?.Data.Accounts == null || !userProfileResponse.Data.Accounts.Any())
            {
                throw new InvalidOperationException("User profile response is empty or invalid.");
            }

            var account = userProfileResponse.Data.Accounts.FirstOrDefault(x => x.Id == accountId);
            if (account == null)
            {
                throw new KeyNotFoundException($"Account with ID {accountId} not found.");
            }

            return account.Balance;
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, string token, object? body = null)
        {
            var request = new HttpRequestMessage(method, url)
            {
                Content = body != null ? JsonContent.Create(body) : null
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _httpClient.SendAsync(request);
        }

        private static string GetEnvironmentVariableOrThrow(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Environment variable '{key}' is not set.");
            }
            return value;
        }
    }
}
