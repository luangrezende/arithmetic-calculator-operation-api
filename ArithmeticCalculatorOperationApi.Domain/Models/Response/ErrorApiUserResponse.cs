using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Domain.Models.Response
{
    public class ErrorApiUserResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}
