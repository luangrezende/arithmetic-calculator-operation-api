namespace ArithmeticCalculatorOperationApi.Domain.Models.DTO
{
    public class OperationTypeDTO
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public decimal Cost { get; set; }
    }
}
