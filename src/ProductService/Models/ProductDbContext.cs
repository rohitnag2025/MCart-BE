using Microsoft.EntityFrameworkCore;

namespace ProductService.Models
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasKey(p => p.ProductId);
            modelBuilder.Entity<Category>().HasKey(c => c.CategoryId);
            modelBuilder.Entity<Category>().HasMany<Category>()
                .WithOne()
                .HasForeignKey(c => c.ParentId)
                .IsRequired(false);

            // Ensure decimal columns are mapped correctly for SQL Server
            modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>().Property(p => p.Discount).HasColumnType("decimal(18,2)");
        }
    }
}