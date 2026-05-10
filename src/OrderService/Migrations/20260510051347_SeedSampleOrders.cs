using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Migrations
{
    /// <inheritdoc />
    public partial class SeedSampleOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert sample orders for two users
            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "OrderId", "UserId", "Status", "TotalAmount", "PaymentMethod", "ShippingAddress", "BillingAddress", "CreatedAt", "Discount" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("BC7C918C-DC91-9F6A-6E04-216906000000"), "Complete", 100.00m, "COD", "Sample Address", "Sample Address", DateTime.UtcNow, 0m },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("027BE9D7-7F56-4E8B-B69F-6A6E04216906"), "Complete", 150.00m, "COD", "Sample Address", "Sample Address", DateTime.UtcNow, 0m }
                });

            // Insert sample order items for each order
            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "OrderItemId", "OrderId", "ProductId", "ProductName", "Quantity", "Price", "Discount" },
                values: new object[,]
                {
                    { new Guid("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new Guid("11111111-1111-1111-1111-111111111111"), new Guid("33333333-3333-3333-3333-333333333333"), "Men's T-Shirt", 2, 25.00m, 5.00m },
                    { new Guid("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new Guid("11111111-1111-1111-1111-111111111111"), new Guid("44444444-4444-4444-4444-444444444444"), "Women's Dress", 1, 50.00m, 10.00m },
                    { new Guid("bbbbbbb1-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new Guid("22222222-2222-2222-2222-222222222222"), new Guid("55555555-5555-5555-5555-555555555555"), "Men's Jeans", 1, 60.00m, 10.00m },
                    { new Guid("bbbbbbb2-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new Guid("22222222-2222-2222-2222-222222222222"), new Guid("66666666-6666-6666-6666-666666666666"), "Women's Skirt", 2, 45.00m, 5.00m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
