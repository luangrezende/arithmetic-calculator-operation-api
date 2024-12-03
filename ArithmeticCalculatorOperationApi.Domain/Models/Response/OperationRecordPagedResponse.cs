using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Domain.Models.Response
{
    public class OperationRecordPagedResponse
    {
        [JsonPropertyName("records")]
        public List<OperationRecordResponse>? Records { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
