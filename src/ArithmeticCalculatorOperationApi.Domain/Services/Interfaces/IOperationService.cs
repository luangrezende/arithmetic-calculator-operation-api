using ArithmeticCalculatorOperationApi.Domain.Models.DTO;

namespace ArithmeticCalculatorOperationApi.Domain.Services.Interfaces
{
    public interface IOperationService
    {
        Task<(string result, string operationValues)> CalculateOperationResult(string operationType, decimal value1, decimal? value2 = null);

        Task<(int totalRecords, List<OperationRecordDTO> records)> GetPagedOperationsAsync(Guid userId, int page, int pageSize, string query);

        Task<bool> SaveOperationRecordAsync(OperationRecordDTO operationRecord);

        Task<bool> SoftDeleteOperationRecordsAsync(Guid userId, List<Guid> recordIds);
    }
}
