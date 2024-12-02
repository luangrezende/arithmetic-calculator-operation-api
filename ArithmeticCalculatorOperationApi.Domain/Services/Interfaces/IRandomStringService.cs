namespace ArithmeticCalculatorOperationApi.Domain.Services.Interfaces
{
    public interface IRandomStringService
    {
        Task<string> GenerateRandomStringAsync();
    }
}
