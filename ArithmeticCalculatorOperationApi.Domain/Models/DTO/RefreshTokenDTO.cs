namespace ArithmeticCalculatorOperationApi.Domain.Models.DTO
{
    public class RefreshTokenDTO
    {
        public Guid UserId { get; set; }

        public string Token { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; } = false;

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        public DateTime? RevokedAt { get; set; }
    }
}
