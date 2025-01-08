using ArithmeticCalculatorOperationApi.Domain.Entities;

namespace ArithmeticCalculatorOperationApi.Application.Interfaces.Repositories
{
    public interface IOperationRepository
    {
        Task<bool> SaveRecordAsync(OperationRecordEntity operationRecord);

        Task<int> GetTotalCountAsync(Guid userId, string query);

        Task<DashboardEntity> GetDashboardDataAsync(Guid userId);

        Task<bool> SoftDeleteOperationRecordsAsync(Guid userId, List<Guid> recordIds);

        Task<List<OperationRecordEntity>> GetPagedOperationsAsync(Guid userId, int page, int pageSize, string query);
    }
}
