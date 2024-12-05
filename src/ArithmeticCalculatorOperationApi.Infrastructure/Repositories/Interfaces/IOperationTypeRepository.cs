using ArithmeticCalculatorOperationApi.Infrastructure.Models;

namespace ArithmeticCalculatorOperationApi.Infrastructure.Repositories.Interfaces
{
    public interface IOperationTypeRepository
    {
        Task<OperationTypeEntity> GetByIdAsync(Guid id);

        Task<List<OperationTypeEntity>> GetByOperatorCodesAsync(string[] operators);

        Task<List<OperationTypeEntity>> GetAllAsync();
    }
}
