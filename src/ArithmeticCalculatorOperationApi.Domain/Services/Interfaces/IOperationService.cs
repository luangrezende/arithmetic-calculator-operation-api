using ArithmeticCalculatorOperationApi.Domain.Models.DTO;

namespace ArithmeticCalculatorOperationApi.Domain.Services.Interfaces
{
    public interface IOperationService
    {
        string CalculateOperation(string expression);

        Task<decimal> CalculateOperationPriceAsync(string expression);

        Task<(int totalRecords, List<OperationRecordDTO> records)> GetPagedOperationsAsync(Guid userId, int page, int pageSize, string query);

        Task<OperationRecordDTO> SaveOperationRecordAsync(OperationRecordDTO operationRecord);

        Task<bool> SoftDeleteOperationRecordsAsync(Guid userId, List<Guid> recordIds);
    }
}
