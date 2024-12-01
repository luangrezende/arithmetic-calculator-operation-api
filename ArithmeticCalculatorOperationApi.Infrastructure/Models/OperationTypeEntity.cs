namespace ArithmeticCalculatorOperationApi.Infrastructure.Models
{
    public class OperationTypeEntity
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public decimal Cost { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
