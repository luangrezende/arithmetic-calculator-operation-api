namespace ArithmeticCalculatorOperationApi.Domain.Models.DTO
{
    public class OperationRecordDTO
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid OperationTypeId { get; set; }

        public string OperationTypeDescription { get; set; }

        public decimal Cost { get; set; }

        public decimal UserBalance { get; set; }

        public string OperationValues { get; set; }

        public string OperationResult { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
