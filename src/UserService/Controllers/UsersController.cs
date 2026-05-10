using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using UserService.Models;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly IConfiguration _config;
        public UsersController(UserDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("external-login/{provider}")]
        public IActionResult ExternalLogin([FromRoute] string provider, [FromQuery] string returnUrl = "/")
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Users", new { returnUrl });
            var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = redirectUrl };
            // Always use 'Twitter' (capital T) as the provider to match the registered scheme
            var scheme = provider.Equals("Twitter", StringComparison.OrdinalIgnoreCase) ? "Twitter" : provider;
            return Challenge(properties, scheme);
        }

        [AllowAnonymous]
        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
        {
            var authenticateResult = await HttpContext.AuthenticateAsync();
            Console.WriteLine($"[DEBUG] ExternalLoginCallback: authenticateResult.Succeeded = {authenticateResult.Succeeded}");
            if (!authenticateResult.Succeeded)
            {
                Console.WriteLine($"[DEBUG] ExternalLoginCallback: Failure = {authenticateResult.Failure}");
                if (authenticateResult.Failure != null)
                {
                    Console.WriteLine($"[DEBUG] Failure Message: {authenticateResult.Failure.Message}");
                    if (authenticateResult.Failure.InnerException != null)
                        Console.WriteLine($"[DEBUG] Inner Exception: {authenticateResult.Failure.InnerException.Message}");
                }
                return Unauthorized();
            }

            // Log all claims for debugging
            if (authenticateResult.Principal != null)
            {
                foreach (var claim in authenticateResult.Principal.Claims)
                {
                    Console.WriteLine($"[DEBUG] Claim: {claim.Type} = {claim.Value}");
                }
            }

            var email = authenticateResult.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var provider = authenticateResult.Properties?.Items[".AuthScheme"];
            var providerUserId = authenticateResult.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("[DEBUG] Email not received from provider");
                return BadRequest("Email not received from provider");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = email,
                    Name = authenticateResult.Principal.Identity.Name ?? email,
                    Provider = provider,
                    ProviderUserId = providerUserId,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true,
                    Role = "Customer"
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            var jwtSecret = _config["Jwt:Secret"];
            var token = UserService.Helpers.JwtHelper.GenerateJwtToken(user.UserId.ToString(), user.Email, user.Role, jwtSecret);
            // Redirect to frontend with token and user info
            var userJson = System.Text.Json.JsonSerializer.Serialize(new {
                userId = user.UserId,
                email = user.Email,
                name = user.Name
            });
            Console.WriteLine($"[DEBUG] User authenticated via {provider}: {email}");
            Console.WriteLine($"[DEBUG] Generated JWT token: {token}");
            Console.WriteLine($"[DEBUG] User info JSON: {userJson}");
            Console.WriteLine($"[DEBUG] Redirecting to frontend with token: {token} and user: {userJson}");
            Console.WriteLine($"[DEBUG] Frontend callback URL: {_config["Frontend:SocialCallbackUrl"]}");
            Console.WriteLine($"[DEBUG] Default frontend callback URL: http://127.0.0.1:4200/auth/social-callback");
            // Get frontend callback base URL from config or use default
            var frontendCallbackBase = _config["Frontend:SocialCallbackUrl"] ?? "http://127.0.0.1:4200/auth/social-callback";
            var redirectUrlWithParams = $"{frontendCallbackBase}?token={Uri.EscapeDataString(token)}&user={Uri.EscapeDataString(userJson)}";
            return Redirect(redirectUrlWithParams);
        }

        [HttpPost("register")]
            public async Task<IActionResult> Register([FromBody] User user, [FromServices] IConfiguration config, [FromServices] IWebHostEnvironment env)
            {
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                    return BadRequest("Email already exists");
                user.UserId = Guid.NewGuid();
                user.CreatedAt = DateTime.UtcNow;
                user.Role = "Customer";
                user.Provider = null;
                user.ProviderUserId = null;
                // Auto-confirm in Development; require email confirmation in Production
                user.EmailConfirmed = env.IsDevelopment();
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if (!env.IsDevelopment())
                {
                    try
                    {
                        var apiKey = config["SendGrid:ApiKey"];
                        var client = new SendGrid.SendGridClient(apiKey);
                        var from = new SendGrid.Helpers.Mail.EmailAddress("no-reply@mcart.com", "MCART Support");
                        var to = new SendGrid.Helpers.Mail.EmailAddress(user.Email);
                        var confirmUrl = $"https://your-frontend-url/confirm-email?email={user.Email}&token={user.UserId}";
                        var msg = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(
                            from, to, "Confirm your email", $"Please confirm your email by clicking this link: {confirmUrl}", $"<strong>Please confirm your email by clicking <a href='{confirmUrl}'>here</a></strong>");
                        await client.SendEmailAsync(msg);
                        return Ok("Registration successful. Please check your email to confirm your account.");
                    }
                    catch
                    {
                        return Ok("Registration successful.");
                    }
                }

                return Ok("Registration successful. You can log in now.");
            }

            [HttpGet("confirm-email")]
            public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] Guid token)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.UserId == token);
                if (user == null) return BadRequest("Invalid confirmation link.");
                user.EmailConfirmed = true;
                await _context.SaveChangesAsync();
                return Ok("Email confirmed. You can now log in.");
            }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req, [FromServices] IConfiguration config)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == req.Email && u.PasswordHash == req.PasswordHash);
            if (user == null) return Unauthorized();
            if (!user.EmailConfirmed) return Unauthorized("Email not confirmed.");
            var jwtSecret = config["Jwt:Secret"];
            var token = UserService.Helpers.JwtHelper.GenerateJwtToken(user.UserId.ToString(), user.Email, user.Role, jwtSecret);
            return Ok(new { token, user });
        }

        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetProfile(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("profile/{id}")]
        public async Task<IActionResult> UpdateProfile(Guid id, User updated)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.Name = updated.Name;
            user.Gender = updated.Gender;
            user.Phone = updated.Phone;
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req, [FromServices] IConfiguration config)
        {
            // Example SendGrid integration
            var apiKey = config["SendGrid:ApiKey"];
            var client = new SendGrid.SendGridClient(apiKey);
            var from = new SendGrid.Helpers.Mail.EmailAddress("no-reply@mcart.com", "MCART Support");
            var to = new SendGrid.Helpers.Mail.EmailAddress(req.Email);
            var msg = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(
                from, to, "Password Reset", "Reset your password", "<strong>Reset your password</strong>");
            await client.SendEmailAsync(msg);
            return Ok("Password reset link sent");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user == null) return NotFound();
            user.PasswordHash = req.NewPasswordHash;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        // ADMIN ENDPOINTS
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpGet("admin/users")]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpGet("admin/users/{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPut("admin/users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] string role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.Role = role;
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpDelete("admin/users/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string NewPasswordHash { get; set; } = string.Empty;
    }
}
