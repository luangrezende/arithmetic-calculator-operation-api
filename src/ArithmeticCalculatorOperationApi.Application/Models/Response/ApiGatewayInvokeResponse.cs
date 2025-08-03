using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Application.Models.Response
{
    public class ApiGatewayInvokeResponse
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("headers")]
        public Dictionary<string, string>? Headers { get; set; } = new();

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("isBase64Encoded")]
        public bool IsBase64Encoded { get; set; }
    }
}
