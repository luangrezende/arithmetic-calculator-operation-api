namespace ArithmeticCalculatorOperationApi.Infrastructure.Models
{
    public class OperationRecordEntity
    {
        public Guid Id { get; set; }

        public Guid OperationTypeId { get; set; }

        public Guid UserId { get; set; }

        public decimal Cost { get; set; }

        public decimal UserBalance { get; set; }

        public string OperationResult { get; set; } = string.Empty;

        public string OperationValues { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
