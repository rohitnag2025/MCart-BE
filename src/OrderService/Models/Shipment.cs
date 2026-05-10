using System;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class Shipment
    {
        [Key]
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Shipped, Delivered, Cancelled
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }
}
