using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using OrderService.Services;
using Stripe;
using Stripe.Checkout;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly OrderEventPublisher _eventPublisher;
        public OrdersController(OrderDbContext context, OrderEventPublisher eventPublisher)
        {
            _context = context;
            _eventPublisher = eventPublisher;
        }

        // CART/WISHLIST
        [HttpGet("wishlist/{userId}")]
        public async Task<IActionResult> GetWishlist(Guid userId)
        {
            var wishlist = await _context.Wishlists.Where(w => w.UserId == userId).ToListAsync();
            return Ok(wishlist);
        }

        [HttpPost("wishlist/add")]
        public async Task<IActionResult> AddToWishlist([FromBody] Wishlist item)
        {
            item.WishlistId = Guid.NewGuid();
            item.CreatedAt = DateTime.UtcNow;
            _context.Wishlists.Add(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPost("wishlist/remove")]
        public async Task<IActionResult> RemoveFromWishlist([FromBody] Wishlist item)
        {
            var entry = await _context.Wishlists.FirstOrDefaultAsync(w => w.UserId == item.UserId && w.ProductId == item.ProductId);
            if (entry == null) return NotFound();
            _context.Wishlists.Remove(entry);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ORDERS
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOrdersByUser(Guid userId)
        {
            var orders = await _context.Orders.Include(o => o.Items).Where(o => o.UserId == userId).ToListAsync();
            return Ok(orders);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] Order order)
        {
            order.OrderId = Guid.NewGuid();
            order.CreatedAt = DateTime.UtcNow;
            order.Status = "Pending";
            foreach (var item in order.Items)
            {
                item.OrderItemId = Guid.NewGuid();
                item.OrderId = order.OrderId;
            }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await _eventPublisher.PublishEventAsync("OrderPlaced", order);
            return Ok(order);
        }

        [HttpPost("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(Guid orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();
            order.Status = "Cancelled";
            await _context.SaveChangesAsync();
            await _eventPublisher.PublishEventAsync("OrderCancelled", order);
            return Ok(order);
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetOrderHistory(Guid userId)
        {
            var orders = await _context.Orders.Include(o => o.Items).Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).ToListAsync();
            return Ok(orders);
        }

        // COUPONS
        [HttpPost("apply-coupon")]
        public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest req)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == req.Code && c.IsActive && c.Expiry > DateTime.UtcNow);
            if (coupon == null) return BadRequest("Invalid or expired coupon");
            await _eventPublisher.PublishEventAsync("CouponApplied", coupon);
            return Ok(coupon);
        }

        [HttpPost("create-payment-intent")]
        public IActionResult CreatePaymentIntent([FromBody] Order order)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(order.TotalAmount * 100), // Stripe expects amount in cents
                Currency = "usd",
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", order.OrderId.ToString() }
                }
            };
            var service = new PaymentIntentService();
            var paymentIntent = service.Create(options);
            return Ok(new { clientSecret = paymentIntent.ClientSecret });
        }
    }

    public class ApplyCouponRequest
    {
        public string Code { get; set; } = string.Empty;
    }
}
