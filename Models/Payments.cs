using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClipperCoffeeCorner.Models
{
    [Table("Payment")]
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required, MaxLength(50)]
        public string Provider { get; set; } = "Square";

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        public Guid IdempotencyKey { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        [MaxLength(500)]
        public string? CheckoutUrl { get; set; }

        [MaxLength(100)]
        public string? ProviderPaymentId { get; set; }

        public Order? Order { get; set; }
    }
}
