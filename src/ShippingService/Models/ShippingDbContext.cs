using Microsoft.EntityFrameworkCore;

namespace ShippingService.Models
{
    public class ShippingDbContext : DbContext
    {
        public ShippingDbContext(DbContextOptions<ShippingDbContext> options) : base(options) { }
        public DbSet<Shipment> Shipments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Shipment>().HasKey(s => s.ShipmentId);
        }
    }
}
