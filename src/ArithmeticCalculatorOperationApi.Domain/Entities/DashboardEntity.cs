namespace ArithmeticCalculatorOperationApi.Domain.Entities
{
    public class DashboardEntity
    {
        public int TotalOperations { get; set; }

        public int TotalMonthlyOperations { get; set; }

        public decimal TotalCredit { get; set; }

        public decimal TotalAnnualCashAdded { get; set; }

        public int TotalPlatformOperations { get; set; }

        public decimal TotalPlatformCashSpent { get; set; }

        public decimal TotalPlatformCashAdded { get; set; }
    }
}
