// Dtos/Auth/RegisterRequest.cs
using System.ComponentModel.DataAnnotations;

namespace ClipperCoffeeCorner.Dtos.Auth
{
    public sealed class RegisterRequest
    {
        [Required, StringLength(32)] public string Username { get; set; } = string.Empty;
        [Required, StringLength(64, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [EmailAddress] public string? Email { get; set; }
        [Phone] public string? PhoneNumber { get; set; }
        public string? ClipperId { get; set; }
    }
}
