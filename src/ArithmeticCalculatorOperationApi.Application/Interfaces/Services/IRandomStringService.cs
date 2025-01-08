namespace ArithmeticCalculatorOperationApi.Application.Interfaces.Services
{
    public interface IRandomStringService
    {
        Task<string> GenerateRandomStringAsync();
    }
}
