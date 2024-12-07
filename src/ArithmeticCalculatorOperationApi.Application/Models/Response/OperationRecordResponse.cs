using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Application.Models.Response
{
    public class OperationRecordResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("cost")]
        public decimal Cost { get; set; }

        [JsonPropertyName("userBalance")]
        public decimal UserBalance { get; set; }

        [JsonPropertyName("expression")]
        public string Expression { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
