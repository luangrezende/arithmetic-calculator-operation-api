using ArithmeticCalculatorOperationApi.Domain.Models.DTO;
using ArithmeticCalculatorOperationApi.Domain.Services.Interfaces;
using ArithmeticCalculatorOperationApi.Infrastructure.Repositories;
using ArithmeticCalculatorOperationApi.Infrastructure.Security;

namespace ArithmeticCalculatorOperationApi.Domain.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDTO?> AuthenticateAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);

            if (user == null)
                return null;

            if (!PasswordHasher.VerifyPassword(password, user.Password!))
                return null;

            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Status = user.Status,
                Name = user.Name,
            };
        }

        public async Task<bool> CreateUserAsync(string username, string password, string name)
        {
            return await _userRepository.CreateUserAsync(username, password, name);
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
        {
            var result = await _userRepository.GetUserByIdAsync(userId);

            return result == null ? null : new UserDTO
            {
                Id = result!.Id,
                Username = result.Username,
                Status = result.Status,
                Name = result.Name,
            };
        }

        public async Task<UserDTO?> GetUserByUsernameAsync(string username)
        {
            var result = await _userRepository.GetUserByUsernameAsync(username);

            return result == null ? null : new UserDTO
            {
                Id = result!.Id,
                Username = result.Username,
                Status = result.Status,
                Name = result.Name,
            };
        }
    }
}
