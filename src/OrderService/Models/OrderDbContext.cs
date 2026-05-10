using Microsoft.EntityFrameworkCore;

namespace OrderService.Models
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasKey(o => o.OrderId);
            modelBuilder.Entity<OrderItem>().HasKey(oi => oi.OrderItemId);
            modelBuilder.Entity<Wishlist>().HasKey(w => w.WishlistId);
            modelBuilder.Entity<Coupon>().HasKey(c => c.CouponId);
            modelBuilder.Entity<Cart>().HasKey(c => c.CartId);
            modelBuilder.Entity<CartItem>().HasKey(ci => ci.CartItemId);

            // Set decimal precision for Order
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>()
                .Property(o => o.Discount)
                .HasColumnType("decimal(18,2)");

            // Set decimal precision for OrderItem
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Discount)
                .HasColumnType("decimal(18,2)");

            // Set decimal precision for Coupon
            modelBuilder.Entity<Coupon>()
                .Property(c => c.Discount)
                .HasColumnType("decimal(18,2)");

            // Set decimal precision for CartItem
            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.Price)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.Discount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(oi => oi.OrderId);
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne()
                .HasForeignKey(ci => ci.CartId);
        }
    }
}