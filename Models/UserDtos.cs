using System.ComponentModel.DataAnnotations;

namespace ClipperCoffeeCorner.Models
{
    public class RegisterUserRequest
    {
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string UserRole { get; set; } = "Customer";

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(20)]
        public string? NotificationPref { get; set; }   // "SMS", "Email", "None"

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class UserResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; } // !!!
        public string NotificationPref { get; set; } = "None"; // !!!
    }
}
