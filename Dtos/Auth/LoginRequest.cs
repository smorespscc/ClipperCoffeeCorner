// Dtos/Auth/LoginRequest.cs
using System.ComponentModel.DataAnnotations;

namespace ClipperCoffeeCorner.Dtos.Auth
{
    public sealed class LoginRequest
    {
        [StringLength(100)] public string? Username { get; set; }
        [Phone] public string? PhoneNumber { get; set; }
        [EmailAddress] public string? Email { get; set; }
        public string? ClipperId { get; set; }
        public string? GroupOrderId { get; set; }

        [Required, StringLength(64, MinimumLength = 6)]
        public string Password { get; set; } = "";
    }
}
