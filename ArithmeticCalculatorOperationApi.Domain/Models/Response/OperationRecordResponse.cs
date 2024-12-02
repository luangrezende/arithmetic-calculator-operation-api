using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Domain.Models.Response
{
    public class OperationRecordResponse
    {
        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("operationTypeId")]
        public Guid OperationTypeId { get; set; }

        [JsonPropertyName("cost")]
        public decimal Cost { get; set; }

        [JsonPropertyName("userBalance")]
        public decimal UserBalance { get; set; }

        [JsonPropertyName("operationValues")]
        public string OperationValues { get; set; }

        [JsonPropertyName("operationResult")]
        public string OperationResult { get; set; }
    }
}
