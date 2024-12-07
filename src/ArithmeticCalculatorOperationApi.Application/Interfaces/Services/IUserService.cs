namespace ArithmeticCalculatorOperationApi.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<decimal> DebitUserBalanceDirectAsync(Guid accountId, decimal operationCost, string token);
    }
}
