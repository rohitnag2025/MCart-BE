using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _context;
        private readonly string _imageRoot = "ProductImages";
        public ProductsController(ProductDbContext context)
        {
            _context = context;
            if (!Directory.Exists(_imageRoot))
                Directory.CreateDirectory(_imageRoot);
        }

        // PUBLIC ENDPOINTS
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] Guid? categoryId, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _context.Products.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search) || p.Tags.Contains(search));
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);
            // Sorting
            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort.ToLower())
                {
                    case "price": query = query.OrderBy(p => p.Price); break;
                    case "-price": query = query.OrderByDescending(p => p.Price); break;
                    case "new": query = query.OrderByDescending(p => p.CreatedAt); break;
                    case "featured": query = query.OrderByDescending(p => p.IsFeatured); break;
                    case "sale": query = query.OrderByDescending(p => p.IsOnSale); break;
                    default: query = query.OrderBy(p => p.Name); break;
                }
            }
            var total = await query.CountAsync();
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return Ok(new { total, products });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeatured()
        {
            var products = await _context.Products.Where(p => p.IsFeatured).ToListAsync();
            return Ok(products);
        }

        [HttpGet("new")]
        public async Task<IActionResult> GetNew()
        {
            var products = await _context.Products.Where(p => p.IsNew).OrderByDescending(p => p.CreatedAt).ToListAsync();
            return Ok(products);
        }

        [HttpGet("sale")]
        public async Task<IActionResult> GetSale()
        {
            var products = await _context.Products.Where(p => p.IsOnSale).ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}/related")]
        public async Task<IActionResult> GetRelated(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            var related = await _context.Products.Where(p => p.CategoryId == product.CategoryId && p.ProductId != id).Take(5).ToListAsync();
            return Ok(related);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(Guid categoryId)
        {
            var products = await _context.Products.Where(p => p.CategoryId == categoryId).ToListAsync();
            return Ok(products);
        }

        // CATEGORY TREE
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        // ADMIN ENDPOINTS
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            product.ProductId = Guid.NewGuid();
            product.CreatedAt = DateTime.UtcNow;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return Ok(product);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Product updated)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            product.Name = updated.Name;
            product.Description = updated.Description;
            product.CategoryId = updated.CategoryId;
            product.Price = updated.Price;
            product.Discount = updated.Discount;
            product.Stock = updated.Stock;
            product.ImageUrl = updated.ImageUrl;
            product.Tags = updated.Tags;
            product.IsFeatured = updated.IsFeatured;
            product.IsNew = updated.IsNew;
            product.IsOnSale = updated.IsOnSale;
            await _context.SaveChangesAsync();
            return Ok(product);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPost("{id}/image")]
        public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");
            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{id}{ext}";
            var filePath = Path.Combine(_imageRoot, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            product.ImageUrl = $"/api/products/image/{fileName}";
            await _context.SaveChangesAsync();
            return Ok(product.ImageUrl);
        }

        [HttpGet("image/{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            var filePath = Path.Combine(_imageRoot, fileName);
            if (!System.IO.File.Exists(filePath)) return NotFound();
            var ext = Path.GetExtension(fileName).ToLower();
            var contentType = ext switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
            return PhysicalFile(filePath, contentType);
        }

        // CATEGORY ADMIN
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            category.CategoryId = Guid.NewGuid();
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] Category updated)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            category.Name = updated.Name;
            category.ParentId = updated.ParentId;
            category.Gender = updated.Gender;
            await _context.SaveChangesAsync();
            return Ok(category);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
