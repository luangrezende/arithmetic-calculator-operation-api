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
            Console.WriteLine($"starting generating random string: URL {Environment.GetEnvironmentVariable("RandomStringEndpoint")}");
            var response = await _httpClient.GetAsync(Environment.GetEnvironmentVariable("RandomStringEndpoint"));

            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.ToString());
            Console.WriteLine(response.Content);

            response.EnsureSuccessStatusCode();

            var randomString = await response.Content.ReadAsStringAsync();
            return randomString.Trim();
        }
    }
}
