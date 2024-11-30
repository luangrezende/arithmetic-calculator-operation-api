using ArithmeticCalculatorOperationApi.Domain.Models.DTO;

namespace ArithmeticCalculatorOperationApi.Domain.Services.Interfaces
{
    public interface ITokenGeneratorService
    {
        string GenerateToken(UserDTO user);
    }
}
