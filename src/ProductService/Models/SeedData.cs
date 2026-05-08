using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Models
{
    public static class SeedData
    {
        public static void Initialize(ProductDbContext context)
        {
            // Seed categories
            if (!context.Categories.Any())
            {
                var categories = new[]
                {
                    // MEN
                    new Category { CategoryId = 1, Name = "Men", Gender = "Men", ParentId = null },
                    new Category { CategoryId = 2, Name = "Clothing", Gender = "Men", ParentId = 1 },
                    new Category { CategoryId = 3, Name = "T-Shirts", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 4, Name = "Casual Shirts", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 5, Name = "Formal Shirts", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 6, Name = "Suits", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 7, Name = "Jeans", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 8, Name = "Casual Trousers", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 9, Name = "Formal Trousers", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 10, Name = "Shorts", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 11, Name = "Track Pants", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 12, Name = "Sweaters & Sweatshirts", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 13, Name = "Jackets", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 14, Name = "Blazers & Coats", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 15, Name = "Sports & Active Wear", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 16, Name = "Indian & Festive Wear", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 17, Name = "Innerwear & Sleepwear", Gender = "Men", ParentId = 2 },
                    new Category { CategoryId = 18, Name = "ACCESSORIES", Gender = "Men", ParentId = 1 },
                    new Category { CategoryId = 19, Name = "Watches & Wearables", Gender = "Men", ParentId = 18 },
                    new Category { CategoryId = 20, Name = "Sunglasses & Frames", Gender = "Men", ParentId = 18 },
                    new Category { CategoryId = 21, Name = "Bags & Backpacks", Gender = "Men", ParentId = 18 },
                    new Category { CategoryId = 22, Name = "Luggage & Trolleys", Gender = "Men", ParentId = 18 },
                    new Category { CategoryId = 23, Name = "Personal Care & Grooming", Gender = "Men", ParentId = 18 },
                    new Category { CategoryId = 24, Name = "Wallets & Belts", Gender = "Men", ParentId = 18 },
                    new Category { CategoryId = 25, Name = "Fashion Accessories", Gender = "Men", ParentId = 18 },

                    // WOMEN (IDs start at 100 to avoid clashes)
                    new Category { CategoryId = 100, Name = "Women", Gender = "Women", ParentId = null },
                    new Category { CategoryId = 101, Name = "Indian & Western Wear", Gender = "Women", ParentId = 100 },
                    new Category { CategoryId = 102, Name = "Kurtas & Suits", Gender = "Women", ParentId = 101 },
                    new Category { CategoryId = 103, Name = "Kurtis & Tunics", Gender = "Women", ParentId = 101 },
                    new Category { CategoryId = 104, Name = "Leggings, Salwars & Churidars", Gender = "Women", ParentId = 101 },
                    new Category { CategoryId = 105, Name = "Skirts & Palazzos", Gender = "Women", ParentId = 101 },
                    new Category { CategoryId = 106, Name = "Sarees & Blouses", Gender = "Women", ParentId = 101 },
                    new Category { CategoryId = 107, Name = "Dress Material", Gender = "Women", ParentId = 101 },
                    new Category { CategoryId = 108, Name = "Lehenga Choli", Gender = "Women", ParentId = 101 },
                    new Category { CategoryId = 109, Name = "Dupattas & Shawls", Gender = "Women", ParentId = 101 },
                    new Category { CategoryId = 110, Name = "Western Wear", Gender = "Women", ParentId = 100 },
                    new Category { CategoryId = 111, Name = "Dresses & Jumpsuits", Gender = "Women", ParentId = 110 },
                    new Category { CategoryId = 112, Name = "Tops, T-Shirts & Shirts", Gender = "Women", ParentId = 110 },
                    new Category { CategoryId = 113, Name = "Jeans & Jeggings", Gender = "Women", ParentId = 110 },
                    new Category { CategoryId = 114, Name = "Trousers & Capris", Gender = "Women", ParentId = 110 },
                    new Category { CategoryId = 115, Name = "Shorts & Skirts", Gender = "Women", ParentId = 110 },
                    new Category { CategoryId = 116, Name = "Shrugs", Gender = "Women", ParentId = 110 },
                    new Category { CategoryId = 117, Name = "Sweaters & Sweatshirts", Gender = "Women", ParentId = 110 },
                    new Category { CategoryId = 118, Name = "Jackets & Waistcoats", Gender = "Women", ParentId = 110 },
                    new Category { CategoryId = 119, Name = "Coats & Blazers", Gender = "Women", ParentId = 110 },
                    new Category { CategoryId = 120, Name = "Accessories", Gender = "Women", ParentId = 100 },
                    new Category { CategoryId = 121, Name = "Women Watches", Gender = "Women", ParentId = 120 },
                    new Category { CategoryId = 122, Name = "Analog", Gender = "Women", ParentId = 121 },
                    new Category { CategoryId = 123, Name = "Chronograph", Gender = "Women", ParentId = 121 },
                    new Category { CategoryId = 124, Name = "Digital", Gender = "Women", ParentId = 121 },
                    new Category { CategoryId = 125, Name = "Analog & Digital", Gender = "Women", ParentId = 121 },
                    new Category { CategoryId = 126, Name = "Sunglasses", Gender = "Women", ParentId = 120 },
                    new Category { CategoryId = 127, Name = "Eye Glasses", Gender = "Women", ParentId = 120 },
                    new Category { CategoryId = 128, Name = "Belt", Gender = "Women", ParentId = 120 }
                };

                using var transaction = context.Database.BeginTransaction();
                try
                {
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Categories ON");
                    context.Categories.AddRange(categories);
                    context.SaveChanges();
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Categories OFF");
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            // Seed products
            if (!context.Products.Any())
            {
                var products = ProductSeedData.GetProducts();
                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }
    }
}
