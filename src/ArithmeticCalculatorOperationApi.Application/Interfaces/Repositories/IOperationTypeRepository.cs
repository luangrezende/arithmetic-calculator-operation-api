using ArithmeticCalculatorOperationApi.Domain.Entities;

namespace ArithmeticCalculatorOperationApi.Application.Interfaces.Repositories
{
    public interface IOperationTypeRepository
    {
        Task<OperationTypeEntity> GetByIdAsync(Guid id);

        Task<List<OperationTypeEntity>> GetByOperatorCodesAsync(string[] operators);

        Task<List<OperationTypeEntity>> GetAllAsync();
    }
}
