using ArithmeticCalculatorOperationApi.Infrastructure.Models;

namespace ArithmeticCalculatorOperationApi.Infrastructure.Repositories.Interfaces
{
    public interface IOperationRepository
    {
        Task<bool> SaveRecordAsync(OperationRecordEntity operationRecord);

        Task<int> GetTotalCountAsync(Guid userId, string query);

        Task<bool> SoftDeleteOperationRecordsAsync(Guid userId, List<Guid> recordIds);

        Task<List<OperationRecordEntity>> GetPagedOperationsAsync(Guid userId, int page, int pageSize, string query);
    }
}
