using ArithmeticCalculatorUserApi.Domain.Models.DTO;

namespace ArithmeticCalculatorUserApi.Domain.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO?> AuthenticateAsync(string username, string password);

        Task<UserDTO?> GetUserByIdAsync(Guid userId);

        Task<UserDTO?> GetUserByUsernameAsync(string username);

        Task<bool> CreateUserAsync(string username, string password, string name);
    }
}
