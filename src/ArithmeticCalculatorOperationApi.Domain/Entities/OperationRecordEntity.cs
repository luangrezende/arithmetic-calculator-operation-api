namespace ArithmeticCalculatorOperationApi.Domain.Entities
{
    public class OperationRecordEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public decimal Cost { get; set; }

        public decimal UserBalance { get; set; }

        public string Result { get; set; } = string.Empty;

        public string Expression { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
