using ArithmeticCalculatorOperationApi.Application.DTOs;
using ArithmeticCalculatorOperationApi.Application.Interfaces.Repositories;
using ArithmeticCalculatorOperationApi.Application.Interfaces.Services;

namespace ArithmeticCalculatorOperationApi.Application.Services
{
    public class OperationTypeService : IOperationTypeService
    {
        private readonly IOperationTypeRepository _operationTypeRepository;

        public OperationTypeService(IOperationTypeRepository operationTypeRepository)
        {
            _operationTypeRepository = operationTypeRepository;
        }

        public async Task<OperationTypeDTO?> GetByIdAsync(Guid id)
        {
            var result = await _operationTypeRepository.GetByIdAsync(id);

            return result == null ? null : new OperationTypeDTO
            {
                Id = result.Id,
                Cost = result.Cost,
                Description = result.Description,
                OperatorCode = result.OperatorCode,
            };
        }

        public async Task<List<OperationTypeDTO>?> GetAllAsync()
        {
            var result = await _operationTypeRepository.GetAllAsync();

            if (result == null || result.Count == 0)
                return null;

            return result.Select(r => new OperationTypeDTO
            {
                Id = r.Id,
                Cost = r.Cost,
                Description = r.Description,
                OperatorCode = r.OperatorCode,
            }).ToList();
        }

    }
}
