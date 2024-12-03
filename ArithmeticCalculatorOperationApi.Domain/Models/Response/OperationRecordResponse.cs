using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Domain.Models.Response
{
    public class OperationRecordResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("operationTypeDescription")]
        public string OperationTypeDescription { get; set; }

        [JsonPropertyName("cost")]
        public decimal Cost { get; set; }

        [JsonPropertyName("userBalance")]
        public decimal UserBalance { get; set; }

        [JsonPropertyName("operationValues")]
        public string OperationValues { get; set; }

        [JsonPropertyName("operationResult")]
        public string OperationResult { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
