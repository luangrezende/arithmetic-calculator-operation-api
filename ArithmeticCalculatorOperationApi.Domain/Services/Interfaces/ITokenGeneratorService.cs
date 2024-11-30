using ArithmeticCalculatorUserApi.Domain.Models.DTO;

namespace ArithmeticCalculatorUserApi.Domain.Services.Interfaces
{
    public interface ITokenGeneratorService
    {
        string GenerateToken(UserDTO user);
    }
}
