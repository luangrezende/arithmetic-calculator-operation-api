using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Domain.Models.Response
{
    public class InnerResponse
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("data")]
        public InnerData? Data { get; set; }
    }
}
