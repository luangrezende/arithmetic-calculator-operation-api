using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ArithmeticCalculatorUserApi.Domain.Models.Request
{
    public class UserRegisterRequest
    {
        [Required(ErrorMessage = "username is required.")]
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "password is required.")]
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "confirmpassword is required.")]
        [JsonPropertyName("confirmPassword")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "name is required.")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        public bool IsValid()
        {
            return string.Equals(Password, ConfirmPassword);
        }
    }
}
