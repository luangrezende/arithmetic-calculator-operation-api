using System.Text.Json.Serialization;

namespace ArithmeticCalculatorOperationApi.Application.Models.Response
{
    public class DashboardResponse
    {
        [JsonPropertyName("totalOperations")]
        public int TotalOperations { get; set; }

        [JsonPropertyName("totalMonthlyOperations")]
        public int TotalMonthlyOperations { get; set; }

        [JsonPropertyName("totalCredit")]
        public decimal TotalCredit { get; set; }

        [JsonPropertyName("totalAnnualCashAdded")]
        public decimal TotalAnnualCashAdded { get; set; }

        [JsonPropertyName("totalPlatformOperations")]
        public int TotalPlatformOperations { get; set; }

        [JsonPropertyName("totalPlatformCashSpent")]
        public decimal TotalPlatformCashSpent { get; set; }

        [JsonPropertyName("totalPlatformCashAdded")]
        public decimal TotalPlatformCashAdded { get; set; } 
        
        [JsonPropertyName("annualTarget")]
        public decimal AnnualTarget { get; set; }
    }
}
