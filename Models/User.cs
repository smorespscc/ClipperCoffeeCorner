using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClipperCoffeeCorner.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string UserRole { get; set; } = "Customer";

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(20)]
        public string NotificationPref { get; set; } = "None";   // "SMS", "Email", "None"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        [MaxLength(200)]
        public string? SquareCustomerId { get; set; }

        // Navigation
        public ICollection<Password> Passwords { get; set; } = new List<Password>();
    }
}