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

        // CART
        [HttpGet("cart/{userId}")]
        public async Task<IActionResult> GetCart(Guid userId)
        {
            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return Ok(new { Items = new List<object>() });
            return Ok(cart);
        }

        [HttpPost("cart/add")]
        public async Task<IActionResult> AddToCart([FromBody] CartItem item)
        {
            // Allow null or Guid.Empty UserId for guest carts
            var userId = item.UserId ?? Guid.Empty;
            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { CartId = Guid.NewGuid(), UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            item.CartItemId = Guid.NewGuid();
            item.CartId = cart.CartId;
            item.UserId = userId;
            _context.CartItems.Add(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPost("cart/remove")]
        public async Task<IActionResult> RemoveFromCart([FromBody] CartItem item)
        {
            // Allow null or Guid.Empty UserId for guest carts
            var userId = item.UserId ?? Guid.Empty;
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return NotFound();
            var entry = await _context.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == item.ProductId);
            if (entry == null) return NotFound();
            _context.CartItems.Remove(entry);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("cart/remove-product")]
        public async Task<IActionResult> RemoveProductFromCart([FromBody] RemoveProductRequest req)
        {
            var userId = req.UserId ?? Guid.Empty;
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return NotFound();
            var entries = await _context.CartItems.Where(ci => ci.CartId == cart.CartId && ci.ProductId == req.ProductId).ToListAsync();
            if (!entries.Any()) return NotFound();
            _context.CartItems.RemoveRange(entries);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("cart/clear/{userId}")]
        public async Task<IActionResult> ClearCart(Guid userId)
        {
            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return Ok();
            _context.CartItems.RemoveRange(cart.Items);
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("cart/assign-user")]
        public async Task<IActionResult> AssignUserToGuestCart([FromBody] AssignUserRequest req)
        {
            // Find guest cart (UserId = Guid.Empty)
            var guestCart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == Guid.Empty);
            if (guestCart == null)
                return NotFound("No guest cart found.");

            // Update cart UserId
            guestCart.UserId = req.UserId;

            // Update all cart items
            var guestItems = await _context.CartItems.Where(ci => ci.UserId == Guid.Empty && ci.CartId == guestCart.CartId).ToListAsync();
            foreach (var item in guestItems)
                item.UserId = req.UserId;

            await _context.SaveChangesAsync();
            return Ok("Cart assigned to user.");
        }

        public class AssignUserRequest
        {
            public Guid UserId { get; set; }
        }

        public class RemoveProductRequest
        {
            public Guid? UserId { get; set; }
            public Guid ProductId { get; set; }
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

        public class CheckoutRequest
        {
            public Guid UserId { get; set; }
            public List<OrderItem> Items { get; set; } = new();
            public string ShippingAddress { get; set; } = string.Empty;
            public string BillingAddress { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = "Stripe";
            public string? CouponCode { get; set; }
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest req)
        {
            // Require billing address
            if (string.IsNullOrWhiteSpace(req.BillingAddress))
                return BadRequest("Billing address is required.");

            // Calculate total
            decimal subtotal = req.Items.Sum(i => i.Price * i.Quantity);
            decimal discount = req.Items.Sum(i => i.Discount * i.Quantity);
            decimal total = subtotal - discount;

            // Create order (not yet confirmed)
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                UserId = req.UserId,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending",
                ShippingAddress = req.ShippingAddress,
                BillingAddress = req.BillingAddress,
                PaymentMethod = req.PaymentMethod,
                CouponCode = req.CouponCode,
                Discount = discount,
                TotalAmount = total,
                Items = req.Items.Select(i => new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Discount = i.Discount
                }).ToList()
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create Stripe session
            var domain = Request.Scheme + "://" + Request.Host.Value;
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = order.Items.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(item.Price * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.ProductName
                        }
                    },
                    Quantity = item.Quantity
                }).ToList(),
                Mode = "payment",
                SuccessUrl = domain + $"/api/orders/confirm-payment?orderId={order.OrderId}",
                CancelUrl = domain + "/payment-cancelled"
            };
            var service = new SessionService();
            var session = service.Create(options);

            // Return Stripe session id to frontend
            return Ok(new { orderId = order.OrderId, stripeSessionId = session.Id, stripePublicKey = Environment.GetEnvironmentVariable("STRIPE_PUBLIC_KEY") });
        }

        // Confirm payment and finalize order
        [HttpGet("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromQuery] Guid orderId)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return NotFound();
            if (order.Status == "Paid") return Ok(new { message = "Order already confirmed." });

            // Here you would verify payment with Stripe webhook or API (omitted for brevity)
            order.Status = "Paid";
            await _context.SaveChangesAsync();

            // TODO: Update inventory in ProductService (call API or publish event)

            // Publish event for courier/shipping
            await _eventPublisher.PublishEventAsync("OrderPaid", order);

            return Ok(new { message = "Payment successful, order placed!", order });
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
        // Update Order Status
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();
            order.Status = status;
            await _context.SaveChangesAsync();
            return Ok(order);
        }

        // Calculate Cart Subtotal/Total
        [HttpPost("calculate")]
        public IActionResult CalculateCart([FromBody] List<OrderItem> items)
        {
            decimal subtotal = items.Sum(i => i.Price * i.Quantity);
            decimal discount = items.Sum(i => i.Discount * i.Quantity);
            decimal total = subtotal - discount;
            return Ok(new { subtotal, discount, total });
        }

        // Order Confirmation
        [HttpGet("{orderId}/confirmation")]
        public async Task<IActionResult> GetOrderConfirmation(Guid orderId)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return NotFound();
            // You can add more confirmation details as needed
            return Ok(new { message = "Order confirmed!", order });
        }

        // Order Shipment Tracking
        [HttpGet("{orderId}/shipment")]
        public async Task<IActionResult> GetShipment(Guid orderId)
        {
            var shipment = await _context.Set<OrderService.Models.Shipment>().FirstOrDefaultAsync(s => s.OrderId == orderId);
            if (shipment == null) return NotFound();
            return Ok(shipment);
        }

        // Admin: Get All Orders
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders.Include(o => o.Items).ToListAsync();
            return Ok(orders);
        }
    }

    public class ApplyCouponRequest
    {
        public string Code { get; set; } = string.Empty;
    }
}
