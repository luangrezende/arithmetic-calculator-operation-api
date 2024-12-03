using ArithmeticCalculatorOperationApi.Domain.Services.Interfaces;

namespace ArithmeticCalculatorOperationApi.Domain.Services
{
    public class RandomStringService : IRandomStringService
    {
        private readonly HttpClient _httpClient;

        public RandomStringService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateRandomStringAsync()
        {
            var response = await _httpClient.GetAsync(Environment.GetEnvironmentVariable("RandomStringEndpoint"));

            response.EnsureSuccessStatusCode();

            var randomString = await response.Content.ReadAsStringAsync();
            return randomString.Trim();
        }
    }
}
