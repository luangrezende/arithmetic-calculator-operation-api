using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Domain.Models.Request
{
    public class AddOperationRequest
    {
        [Required(ErrorMessage = "operationTypeId is required.")]
        [JsonPropertyName("operationTypeId")]
        public Guid OperationTypeId { get; set; }

        [Required(ErrorMessage = "accountId is required.")]
        [JsonPropertyName("accountId")]
        public Guid AccountId { get; set; }

        [JsonPropertyName("value1")]
        public decimal Value1 { get; set; }

        [JsonPropertyName("value2")]
        public decimal? Value2 { get; set; }
    }
}
