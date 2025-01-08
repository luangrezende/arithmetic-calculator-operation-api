using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Application.Models.Request
{
    public class AddOperationRequest
    {
        [Required(ErrorMessage = "accountId is required.")]
        [JsonPropertyName("accountId")]
        public Guid AccountId { get; set; }

        [JsonPropertyName("expression")]
        public string? Expression { get; set; }
    }
}
