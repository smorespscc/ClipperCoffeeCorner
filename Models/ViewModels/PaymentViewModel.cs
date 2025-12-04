using ClipperCoffeeCorner.Models.Domain;
using System.Collections.Generic;
using System.Linq;

namespace ClipperCoffeeCorner.Models.ViewModels
{
    public class PaymentViewModel
    {
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public decimal Subtotal { get; set; }
        public decimal StaffDiscount { get; set; }
        public decimal Total { get; set; }
        public bool IsStaff { get; set; }
        public string? GlobalSpecialRequests { get; set; }
    }
}
