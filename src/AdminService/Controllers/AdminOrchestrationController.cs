using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Threading.Tasks;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/admin/orchestration")]
    [Authorize(Roles = "Admin")]
    public class AdminOrchestrationController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public AdminOrchestrationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetAllProducts()
        {
            var client = _httpClientFactory.CreateClient("ProductService");
            var response = await client.GetAsync("/api/products");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var client = _httpClientFactory.CreateClient("OrderService");
            var response = await client.GetAsync("/api/orders");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var client = _httpClientFactory.CreateClient("UserService");
            var response = await client.GetAsync("/api/users");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var productClient = _httpClientFactory.CreateClient("ProductService");
            var orderClient = _httpClientFactory.CreateClient("OrderService");
            var userClient = _httpClientFactory.CreateClient("UserService");
            var products = await productClient.GetStringAsync("/api/products");
            var orders = await orderClient.GetStringAsync("/api/orders");
            var users = await userClient.GetStringAsync("/api/users");
            return Ok(new { products, orders, users });
        }
    }
}
