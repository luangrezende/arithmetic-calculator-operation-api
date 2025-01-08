using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Application.Models.Response
{
    public class OperationTypeResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("operationCode")]
        public string OperatorCode { get; set; }

        [JsonPropertyName("cost")]
        public decimal Cost { get; set; }
    }
}
