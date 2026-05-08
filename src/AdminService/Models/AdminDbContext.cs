using Microsoft.EntityFrameworkCore;

namespace AdminService.Models
{
    public class AdminDbContext : DbContext
    {
        public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }
        public DbSet<AdminUser> AdminUsers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdminUser>().HasKey(a => a.AdminUserId);
            modelBuilder.Entity<AdminUser>().HasIndex(a => a.Email).IsUnique();
        }
    }
}