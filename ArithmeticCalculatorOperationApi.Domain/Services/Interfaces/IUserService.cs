namespace ArithmeticCalculatorOperationApi.Domain.Services.Interfaces
{
    public interface IUserService
    {
        Task<HttpResponseMessage> DebitUserBalanceAsync(Guid accountId, decimal amount, string token);
        
        Task<decimal> GetUserBalanceAsync(Guid accountId, string token);
    }
}
