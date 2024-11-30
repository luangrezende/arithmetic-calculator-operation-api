
namespace ArithmeticCalculatorUserApi.Infrastructure.Models
{
    public class RefreshTokenEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public string Token { get; set; } = Guid.NewGuid().ToString();

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; } = false;

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }
    }
}
