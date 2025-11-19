using System;
using System.ComponentModel.DataAnnotations;

namespace ClipperCoffeeCorner.Models
{
    public class Customer
    {
        [Key]
        public Guid Id { get; set; } // Unique identifier for the customer

        [Required]
        [EmailAddress]
        public required string Email { get; set; } // Customer's email or ClipperID

        public required string SquareCustomerId { get; set; } // Square-specific customer ID

        [Phone]
        public required string PhoneNumber { get; set; } // Customer's phone number
    }
}