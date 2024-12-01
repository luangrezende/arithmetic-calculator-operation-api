using ArithmeticCalculatorOperationApi.Domain.Models.DTO;

namespace ArithmeticCalculatorOperationApi.Domain.Services.Interfaces
{
    public interface IOperationService
    {
        Task<(string result, string operationValues)> CalculateOperationResult(string operationType, decimal value1, decimal? value2 = null);

        Task<bool> SaveOperationRecordAsync(OperationRecordDTO operationRecord);
    }
}
