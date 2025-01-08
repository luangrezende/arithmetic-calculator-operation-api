using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Application.Models.Request
{
    public class SoftDeleteOperationRecordRequest
    {
        [JsonPropertyName("ids")]
        public List<Guid> Ids { get; set; }
    }
}
