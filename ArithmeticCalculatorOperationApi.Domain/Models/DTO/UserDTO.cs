using ArithmeticCalculatorOperationApi.Domain.Enums;
using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Domain.Models.DTO
{
    public class UserDTO
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("accounts")]
        public IEnumerable<BankAccountDTO> Accounts { get; set; }

        public bool IsActive()
        {
            return Status?.ToString()!.Equals(UserStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }
}
