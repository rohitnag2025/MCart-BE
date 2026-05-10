using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Models;
using Stripe;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentDbContext _context;
        public PaymentsController(PaymentDbContext context)
        {
            _context = context;
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetPaymentByOrderId(Guid orderId)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetPayment(Guid paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var secret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], secret);
            }
            catch
            {
                return BadRequest();
            }

            if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                var intent = (PaymentIntent)stripeEvent.Data.Object;
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id);
                if (payment != null)
                {
                    payment.Status = "Succeeded";
                    await _context.SaveChangesAsync();
                }
            }
            else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
            {
                var intent = (PaymentIntent)stripeEvent.Data.Object;
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id);
                if (payment != null)
                {
                    payment.Status = "Failed";
                    await _context.SaveChangesAsync();
                }
            }
            return Ok();
        }
    }
}
