using ArithmeticCalculatorOperationApi.Domain.Models.DTO;

namespace ArithmeticCalculatorOperationApi.Domain.Services.Interfaces
{
    public interface IOperationTypeService
    {
        Task<OperationTypeDTO?> GetByIdAsync(Guid id);
    }
}
