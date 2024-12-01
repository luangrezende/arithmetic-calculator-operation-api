namespace ArithmeticCalculatorOperationApi.Domain.Models.Response
{
    public class OperationRecordResponse
    {
        public Guid UserId { get; set; }

        public Guid OperationTypeId { get; set; }

        public decimal Cost { get; set; }

        public decimal UserBalance { get; set; }

        public string OperationValues { get; set; }

        public string OperationResult { get; set; }
    }
}
