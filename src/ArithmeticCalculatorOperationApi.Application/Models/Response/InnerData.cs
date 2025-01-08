using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Application.Models.Response
{
    public class InnerData
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}
