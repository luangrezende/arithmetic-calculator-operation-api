using ArithmeticCalculatorOperationApi.Application.DTOs;
using ArithmeticCalculatorOperationApi.Application.Models.Response;

namespace ArithmeticCalculatorOperationApi.Application.Interfaces.Services
{
    public interface IOperationService
    {
        Task<string> CalculateOperation(string expression);

        Task<decimal> CalculateOperationPriceAsync(string expression);

        Task<DashboardResponse> GetDashboardDataAsync(Guid userId);

        Task<(int totalRecords, List<OperationRecordDTO> records)> GetPagedOperationsAsync(Guid userId, int page, int pageSize, string query);

        Task<OperationRecordDTO> SaveOperationRecordAsync(OperationRecordDTO operationRecord);

        Task<bool> SoftDeleteOperationRecordsAsync(Guid userId, List<Guid> recordIds);
    }
}
