using System;
using System.ComponentModel.DataAnnotations;

namespace ProductService.Models
{
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int Stock { get; set; }
        // Stores only the blob name (not a URL)
        public string ImageBlobName { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public bool IsFeatured { get; set; }
        public bool IsNew { get; set; }
        public bool IsOnSale { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}