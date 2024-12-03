namespace ArithmeticCalculatorOperationApi.Domain.Services.Interfaces
{
    public interface IUserService
    {
        Task<decimal> DebitUserBalanceDirectAsync(Guid accountId, decimal operationCost, string token);
    }
}
