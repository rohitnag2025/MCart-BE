using System;
using System.ComponentModel.DataAnnotations;

namespace ShippingService.Models
{
    public class Shipment
    {
        [Key]
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Shipped, Delivered
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
