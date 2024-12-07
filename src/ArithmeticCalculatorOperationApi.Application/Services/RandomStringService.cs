using ArithmeticCalculatorOperationApi.Application.Interfaces.Services;

namespace ArithmeticCalculatorOperationApi.Application.Services
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
            var response = await _httpClient.GetAsync(Environment.GetEnvironmentVariable("RANDOM_STRING_ENDPOINT"));

            response.EnsureSuccessStatusCode();

            var randomString = await response.Content.ReadAsStringAsync();
            return randomString.Trim();
        }
    }
}
