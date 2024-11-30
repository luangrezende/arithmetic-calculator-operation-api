namespace ArithmeticCalculatorOperationApi.Infrastructure.Models
{
    public class UserEntity
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Name { get; set; }

        public string Status { get; set; }

        public string Password { get; set; }

        public IEnumerable<BankAccountEntity> Accounts { get; set; }
    }
}
