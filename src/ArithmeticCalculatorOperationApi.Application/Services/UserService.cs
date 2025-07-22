using ArithmeticCalculatorOperationApi.Application.Interfaces.Services;
using ArithmeticCalculatorOperationApi.Application.Models.Response;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserService> _logger;

    public UserService(HttpClient httpClient, ILogger<UserService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<decimal> DebitUserBalanceDirectAsync(Guid accountId, decimal operationCost, string token)
    {
        _logger.LogInformation("DebitUserBalanceDirectAsync started: accountId={AccountId}, operationCost={OperationCost}", accountId, operationCost);

        var debitUrl = Environment.GetEnvironmentVariable("USER_DEBIT_API_URL")
            ?? throw new InvalidOperationException("Missing USER_DEBIT_API_URL environment variable.");

        var body = new
        {
            accountId = accountId.ToString(),
            amount = operationCost
        };

        var requestContent = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PutAsync(debitUrl, requestContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during HTTP PUT to debit endpoint.");
            throw;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Response from Debit API: StatusCode={StatusCode}, Body={Body}", response.StatusCode, responseBody);

        if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(responseBody))
        {
            try
            {
                var error = JsonSerializer.Deserialize<InnerResponse<ErrorApiUserResponse>>(responseBody);
                throw new InvalidOperationException(error?.Data?.Error ?? "Unknown error from Debit API");
            }
            catch
            {
                throw new InvalidOperationException("Error parsing error response from Debit API.");
            }
        }

        return await GetUpdatedBalanceAsync(accountId, token);
    }

    private async Task<decimal> GetUpdatedBalanceAsync(Guid accountId, string token)
    {
        _logger.LogInformation("GetUpdatedBalanceAsync started: accountId={AccountId}", accountId);

        var profileUrl = Environment.GetEnvironmentVariable("USER_PROFILE_API_URL")
            ?? throw new InvalidOperationException("Missing USER_PROFILE_API_URL environment variable.");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(profileUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during HTTP GET to profile endpoint.");
            throw;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Response from Profile API: StatusCode={StatusCode}, Body={Body}", response.StatusCode, responseBody);

        if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(responseBody))
            throw new InvalidOperationException("Failed to retrieve user profile.");

        var profile = JsonSerializer.Deserialize<UserApiResponse<UserProfileResponse>>(responseBody);
        var account = profile?.Data?.Accounts?.FirstOrDefault(a => a.Id == accountId);

        if (account == null)
        {
            _logger.LogError("Account with ID {AccountId} not found in profile response", accountId);
            throw new KeyNotFoundException($"Account with ID {accountId} not found.");
        }

        _logger.LogInformation("Updated balance retrieved: {Balance}", account.Balance);
        return account.Balance;
    }
}
