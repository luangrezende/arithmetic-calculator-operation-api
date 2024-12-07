namespace ArithmeticCalculatorOperationApi.Application.DTOs
{
    public class OperationTypeDTO
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public string OperatorCode { get; set; }

        public decimal Cost { get; set; }
    }
}
