namespace ArithmeticCalculatorOperationApi.Application.DTOs
{
    public class OperationRecordDTO
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public decimal Cost { get; set; }

        public decimal UserBalance { get; set; }

        public string Expression { get; set; }

        public string Result { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
