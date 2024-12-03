using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Domain.Models.Request
{
    public class SoftDeleteOperationRecordRequest
    {
        [JsonPropertyName("ids")]
        public List<Guid> Ids { get; set; }
    }
}
