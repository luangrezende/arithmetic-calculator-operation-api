using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Domain.Models.Response
{
    public class InnerData
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}
