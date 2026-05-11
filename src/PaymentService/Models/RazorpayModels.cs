using System.Text.Json.Serialization;

namespace PaymentService.Models
{
    public class RazorpayOrderRequest
    {
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "INR";
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class RazorpayOrderResponse
    {
        public string Id { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Currency { get; set; } = "INR";
    }

    public class RazorpayVerifyRequest
    {
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
    }
}
