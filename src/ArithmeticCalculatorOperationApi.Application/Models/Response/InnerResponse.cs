using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Application.Models.Response
{
    public class InnerResponse<T>
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
