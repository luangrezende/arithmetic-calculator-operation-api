using System.Text.Json.Serialization;

namespace ArithmeticCalculatorUserApi.Domain.Models.Response
{
    public class TokenResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("expiration")]
        public int Expiration { get; set; }
    }
}
