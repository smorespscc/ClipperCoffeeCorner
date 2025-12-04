using System.ComponentModel.DataAnnotations;

namespace ClipperCoffeeCorner.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool IsStaff { get; set; }
        public string? StaffCode { get; set; }
    }
}
