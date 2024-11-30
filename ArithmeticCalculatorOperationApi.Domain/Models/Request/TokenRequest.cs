using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ArithmeticCalculatorUserApi.Domain.Models.Request
{
    public class TokenRequest
    {
        [Required(ErrorMessage = "username is required.")]
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "password is required.")]
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
