using ArithmeticCalculatorOperationApi.Infrastructure.Models;

namespace ArithmeticCalculatorOperationApi.Infrastructure.Repositories.Interfaces
{
    public interface IOperationRepository
    {
        Task<bool> SaveOperationRecordAsync(OperationRecordEntity operationRecord);
    }
}
