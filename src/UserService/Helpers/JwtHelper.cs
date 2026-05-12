using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace UserService.Helpers
{
    public static class JwtHelper
    {
        public static string GenerateJwtToken(string userId, string email, string role, string jwtSecret, int expireMinutes = 60)
        {
            if (string.IsNullOrEmpty(jwtSecret)) throw new ArgumentException("JWT secret is required", nameof(jwtSecret));

            var tokenHandler = new JwtSecurityTokenHandler();

            // Ensure key is large enough for HMAC-SHA256 (>= 256 bits). If the provided secret is too short,
            // derive a 256-bit key by hashing the secret with SHA-256.
            var keyBytes = Encoding.UTF8.GetBytes(jwtSecret);
            if (keyBytes.Length < 32)
            {
                keyBytes = SHA256.HashData(keyBytes);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
