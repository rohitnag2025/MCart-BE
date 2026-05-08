using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Gender { get; set; } // Men, Women, Unisex, Boys, Girls, etc.
        public int? ParentId { get; set; }
    }
}
