using ArithmeticCalculatorOperationApi.Infrastructure.Models;

namespace ArithmeticCalculatorOperationApi.Infrastructure.Repositories.Interfaces
{
    public interface IOperationTypeRepository
    {
        Task<OperationTypeEntity> GetByIdAsync(Guid id);
    }
}
