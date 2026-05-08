using System;
using System.ComponentModel.DataAnnotations;

namespace AdminService.Models
{
    public class AdminUser
    {
        [Key]
        public Guid AdminUserId { get; set; }
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}