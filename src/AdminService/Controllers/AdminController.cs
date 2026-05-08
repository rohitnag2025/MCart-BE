using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminService.Models;
using System;
using System.Threading.Tasks;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AdminDbContext _context;
        public AdminController(AdminDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AdminUser user)
        {
            if (await _context.AdminUsers.AnyAsync(a => a.Email == user.Email))
                return BadRequest("Email already exists");
            user.AdminUserId = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;
            user.Role = "Admin";
            _context.AdminUsers.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Email == req.Email && a.PasswordHash == req.PasswordHash);
            if (user == null) return Unauthorized();
            // TODO: Generate JWT token
            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.AdminUsers.ToListAsync());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _context.AdminUsers.FindAsync(id);
            if (user == null) return NotFound();
            _context.AdminUsers.Remove(user);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
