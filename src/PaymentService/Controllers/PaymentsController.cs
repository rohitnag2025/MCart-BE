using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly string _razorpayKey;
        private readonly string _razorpaySecret;
        private readonly PaymentDbContext _context;
        public PaymentsController(PaymentDbContext context, IConfiguration config)
        {
            _context = context;
            _razorpayKey = config["Razorpay:Key"] ?? "YOUR_RAZORPAY_KEY";
            _razorpaySecret = config["Razorpay:Secret"] ?? "YOUR_RAZORPAY_SECRET";
        }

        [HttpPost("create-razorpay-order")]
        public async Task<IActionResult> CreateRazorpayOrder([FromBody] RazorpayOrderRequest req)
        {
            using var client = new HttpClient();
            Console.WriteLine($"Creating Razorpay order for amount: {req.Amount} {req.Currency}");
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_razorpayKey}:{_razorpaySecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            var payload = new
            {
                amount = req.Amount, // amount in paise
                currency = req.Currency,
                receipt = Guid.NewGuid().ToString(),
                payment_capture = 1
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.razorpay.com/v1/orders", content);
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Razorpay response: {body}");
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, body);
            var order = JsonSerializer.Deserialize<JsonElement>(body);
            Console.WriteLine($"Razorpay order created with ID: {order.GetProperty("id").GetString()}");
            return Ok(new RazorpayOrderResponse
            {
                Id = order.GetProperty("id").GetString()!,
                Amount = order.GetProperty("amount").GetInt32(),
                Currency = order.GetProperty("currency").GetString() ?? "INR"
            });
        }

        [HttpPost("verify-razorpay")]
        public IActionResult VerifyRazorpay([FromBody] JsonElement payload)
        {
            // Extract fields from payload
            var paymentId = payload.GetProperty("razorpay_payment_id").GetString();
            var orderId = payload.GetProperty("razorpay_order_id").GetString();
            var signature = payload.GetProperty("razorpay_signature").GetString();
            var generatedSignature = GenerateSignature(orderId + "|" + paymentId, _razorpaySecret);
            if (generatedSignature == signature)
            {
                // Mark payment as successful in DB if needed
                return Ok(new { success = true });
            }
            return Ok(new { success = false });
        }

        private static string GenerateSignature(string data, string key)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
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
    }
}
