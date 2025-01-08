using ArithmeticCalculatorOperationApi.Application.DTOs;

namespace ArithmeticCalculatorOperationApi.Application.Interfaces.Services
{
    public interface IOperationTypeService
    {
        Task<OperationTypeDTO?> GetByIdAsync(Guid id);

        Task<List<OperationTypeDTO>?> GetAllAsync();
    }
}
