using System;
using System.Collections.Generic;
using ProductService.Models;

namespace ProductService.Models
{
    public static class ProductSeedData
    {
        public static IEnumerable<Product> GetProducts()
        {
            var products = new List<Product>();

            // Men Categories
            products.AddRange(GenerateProductsForCategory(3, "Men's Classic T-Shirt", "Classic cotton t-shirt for men.", "mens-tshirt.jpg", "tshirt,men,clothing", 19.99m, 0, 100));
            products.AddRange(GenerateProductsForCategory(4, "Men's Casual Shirt", "Casual checked shirt for men.", "mens-casualshirt.jpg", "shirt,men,casual", 29.99m, 5, 80));
            products.AddRange(GenerateProductsForCategory(5, "Men's Formal Shirt", "Formal white shirt for men.", "mens-formalshirt.jpg", "shirt,men,formal", 34.99m, 10, 60));
            products.AddRange(GenerateProductsForCategory(6, "Men's Suit", "Slim fit suit for men.", "mens-suit.jpg", "suit,men,formal", 99.99m, 15, 30));
            products.AddRange(GenerateProductsForCategory(7, "Men's Jeans", "Blue denim jeans for men.", "mens-jeans.jpg", "jeans,men,clothing", 39.99m, 0, 120));
            products.AddRange(GenerateProductsForCategory(8, "Men's Casual Trousers", "Comfortable casual trousers.", "mens-casualtrousers.jpg", "trousers,men,casual", 27.99m, 0, 90));
            products.AddRange(GenerateProductsForCategory(9, "Men's Formal Trousers", "Formal black trousers.", "mens-formaltrousers.jpg", "trousers,men,formal", 32.99m, 5, 70));
            products.AddRange(GenerateProductsForCategory(10, "Men's Shorts", "Cotton shorts for men.", "mens-shorts.jpg", "shorts,men,casual", 15.99m, 0, 110));
            products.AddRange(GenerateProductsForCategory(11, "Men's Track Pants", "Track pants for workouts.", "mens-trackpants.jpg", "trackpants,men,sports", 22.99m, 0, 95));
            products.AddRange(GenerateProductsForCategory(12, "Men's Sweatshirt", "Warm sweatshirt for men.", "mens-sweatshirt.jpg", "sweatshirt,men,winter", 24.99m, 10, 85));
            products.AddRange(GenerateProductsForCategory(13, "Men's Jacket", "Leather jacket for men.", "mens-jacket.jpg", "jacket,men,winter", 59.99m, 20, 40));
            products.AddRange(GenerateProductsForCategory(14, "Men's Blazer", "Formal blazer for men.", "mens-blazer.jpg", "blazer,men,formal", 79.99m, 10, 35));
            products.AddRange(GenerateProductsForCategory(15, "Men's Sportswear", "Sportswear set for men.", "mens-sportswear.jpg", "sportswear,men,sports", 49.99m, 5, 60));
            products.AddRange(GenerateProductsForCategory(16, "Men's Kurta", "Traditional kurta for men.", "mens-kurta.jpg", "kurta,men,festive", 29.99m, 0, 50));
            products.AddRange(GenerateProductsForCategory(17, "Men's Pajama Set", "Cotton pajama set.", "mens-pajama.jpg", "pajama,men,sleepwear", 19.99m, 0, 70));
            // Accessories (Men)
            products.AddRange(GenerateProductsForCategory(19, "Men's Watch", "Analog wrist watch.", "mens-watch.jpg", "watch,men,accessory", 99.99m, 0, 40));
            products.AddRange(GenerateProductsForCategory(20, "Men's Sunglasses", "Polarized sunglasses.", "mens-sunglasses.jpg", "sunglasses,men,accessory", 49.99m, 0, 60));
            products.AddRange(GenerateProductsForCategory(21, "Men's Backpack", "Casual backpack.", "mens-backpack.jpg", "backpack,men,bag", 39.99m, 0, 30));
            products.AddRange(GenerateProductsForCategory(22, "Men's Trolley", "Travel trolley bag.", "mens-trolley.jpg", "trolley,men,bag", 89.99m, 0, 20));
            products.AddRange(GenerateProductsForCategory(23, "Men's Grooming Kit", "Complete grooming kit.", "mens-grooming.jpg", "grooming,men,personalcare", 29.99m, 0, 50));
            products.AddRange(GenerateProductsForCategory(24, "Men's Wallet", "Leather wallet.", "mens-wallet.jpg", "wallet,men,accessory", 19.99m, 0, 100));
            products.AddRange(GenerateProductsForCategory(25, "Men's Belt", "Formal leather belt.", "mens-belt.jpg", "belt,men,accessory", 14.99m, 0, 90));

            // Women Categories
            products.AddRange(GenerateProductsForCategory(102, "Women's Kurta Suit", "Designer kurta suit.", "womens-kurta.jpg", "kurta,women,clothing", 49.99m, 10, 50));
            products.AddRange(GenerateProductsForCategory(103, "Women's Tunic", "Printed tunic for women.", "womens-tunic.jpg", "tunic,women,clothing", 29.99m, 0, 60));
            products.AddRange(GenerateProductsForCategory(104, "Women's Leggings", "Cotton leggings.", "womens-leggings.jpg", "leggings,women,clothing", 15.99m, 0, 80));
            products.AddRange(GenerateProductsForCategory(105, "Women's Palazzo", "Palazzo pants.", "womens-palazzo.jpg", "palazzo,women,clothing", 24.99m, 0, 70));
            products.AddRange(GenerateProductsForCategory(106, "Women's Saree", "Silk saree.", "womens-saree.jpg", "saree,women,clothing", 99.99m, 20, 30));
            products.AddRange(GenerateProductsForCategory(107, "Women's Dress Material", "Unstitched dress material.", "womens-dressmaterial.jpg", "dressmaterial,women,clothing", 34.99m, 0, 40));
            products.AddRange(GenerateProductsForCategory(108, "Women's Lehenga", "Bridal lehenga.", "womens-lehenga.jpg", "lehenga,women,clothing", 199.99m, 25, 10));
            products.AddRange(GenerateProductsForCategory(109, "Women's Dupatta", "Embroidered dupatta.", "womens-dupatta.jpg", "dupatta,women,clothing", 19.99m, 0, 60));
            products.AddRange(GenerateProductsForCategory(111, "Women's Dress", "Evening dress.", "womens-dress.jpg", "dress,women,clothing", 59.99m, 10, 40));
            products.AddRange(GenerateProductsForCategory(112, "Women's Top", "Casual top for women.", "womens-top.jpg", "top,women,clothing", 24.99m, 0, 70));
            products.AddRange(GenerateProductsForCategory(113, "Women's Jeans", "Skinny jeans.", "womens-jeans.jpg", "jeans,women,clothing", 39.99m, 0, 90));
            products.AddRange(GenerateProductsForCategory(114, "Women's Trousers", "Formal trousers.", "womens-trousers.jpg", "trousers,women,clothing", 32.99m, 0, 60));
            products.AddRange(GenerateProductsForCategory(115, "Women's Skirt", "Pleated skirt.", "womens-skirt.jpg", "skirt,women,clothing", 27.99m, 0, 50));
            products.AddRange(GenerateProductsForCategory(116, "Women's Shrug", "Knitted shrug.", "womens-shrug.jpg", "shrug,women,clothing", 19.99m, 0, 40));
            products.AddRange(GenerateProductsForCategory(117, "Women's Sweatshirt", "Winter sweatshirt.", "womens-sweatshirt.jpg", "sweatshirt,women,winter", 29.99m, 0, 55));
            products.AddRange(GenerateProductsForCategory(118, "Women's Jacket", "Denim jacket.", "womens-jacket.jpg", "jacket,women,clothing", 49.99m, 0, 35));
            products.AddRange(GenerateProductsForCategory(119, "Women's Blazer", "Formal blazer.", "womens-blazer.jpg", "blazer,women,formal", 69.99m, 0, 25));
            products.AddRange(GenerateProductsForCategory(121, "Women's Watch", "Rose gold watch.", "womens-watch.jpg", "watch,women,accessory", 129.99m, 0, 30));
            products.AddRange(GenerateProductsForCategory(122, "Women's Analog Watch", "Analog wrist watch.", "womens-analogwatch.jpg", "watch,women,analog", 89.99m, 0, 40));
            products.AddRange(GenerateProductsForCategory(123, "Women's Chronograph Watch", "Chronograph watch.", "womens-chronograph.jpg", "watch,women,chronograph", 149.99m, 0, 20));
            products.AddRange(GenerateProductsForCategory(124, "Women's Digital Watch", "Digital watch.", "womens-digitalwatch.jpg", "watch,women,digital", 59.99m, 0, 50));
            products.AddRange(GenerateProductsForCategory(125, "Women's Analog & Digital Watch", "Analog & digital combo watch.", "womens-analogdigital.jpg", "watch,women,analogdigital", 99.99m, 0, 30));
            products.AddRange(GenerateProductsForCategory(126, "Women's Sunglasses", "Cat-eye sunglasses.", "womens-sunglasses.jpg", "sunglasses,women,accessory", 49.99m, 0, 60));
            products.AddRange(GenerateProductsForCategory(127, "Women's Eye Glasses", "Prescription eye glasses.", "womens-eyeglasses.jpg", "eyeglasses,women,accessory", 39.99m, 0, 45));
            products.AddRange(GenerateProductsForCategory(128, "Women's Belt", "Leather belt for women.", "womens-belt.jpg", "belt,women,accessory", 19.99m, 0, 70));

            return products;
        }

        private static IEnumerable<Product> GenerateProductsForCategory(int categoryId, string name, string description, string image, string tags, decimal price, decimal discount, int stock)
        {
            return new[]
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    CategoryId = categoryId,
                    Price = price,
                    Discount = discount,
                    Stock = stock,
                    ImageBlobName = image,
                    Tags = tags,
                    IsFeatured = true,
                    IsNew = true,
                    IsOnSale = discount > 0
                },
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    Name = name + " (Variant)",
                    Description = description + " (Different color/size)",
                    CategoryId = categoryId,
                    Price = price + 5,
                    Discount = discount,
                    Stock = stock / 2,
                    ImageBlobName = image.Replace(".jpg", "-variant.jpg"),
                    Tags = tags + ",variant",
                    IsFeatured = false,
                    IsNew = false,
                    IsOnSale = discount > 0
                }
            };
        }
    }
}
