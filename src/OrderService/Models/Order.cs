using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class Cart
    {
        [Key]
        public Guid CartId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }

    public class CartItem
    {
        [Key]
        public Guid CartItemId { get; set; }
        public Guid CartId { get; set; }
        public Guid? UserId { get; set; } // Nullable for guest carts
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
    }

    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Complete, Cancelled
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public string? CouponCode { get; set; }
        public decimal Discount { get; set; }
    }

    public class OrderItem
    {
        [Key]
        public Guid OrderItemId { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
    }

    public class Wishlist
    {
        [Key]
        public Guid WishlistId { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Coupon
    {
        [Key]
        public Guid CouponId { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Discount { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsActive { get; set; } = true;
    }
}