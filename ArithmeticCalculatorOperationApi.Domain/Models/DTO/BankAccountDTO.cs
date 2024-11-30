namespace ArithmeticCalculatorUserApi.Domain.Models.DTO
{
    public class BankAccountDTO
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public decimal Balance { get; set; }

        public string Currency { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
