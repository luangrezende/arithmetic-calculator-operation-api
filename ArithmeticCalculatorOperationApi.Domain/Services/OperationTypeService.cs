﻿using ArithmeticCalculatorOperationApi.Domain.Models.DTO;
using ArithmeticCalculatorOperationApi.Domain.Services.Interfaces;
using ArithmeticCalculatorOperationApi.Infrastructure.Repositories.Interfaces;

namespace ArithmeticCalculatorOperationApi.Domain.Services
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
            };
        }
    }
}
