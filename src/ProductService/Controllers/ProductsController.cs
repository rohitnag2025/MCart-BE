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
        private readonly AzureBlobService _blobService;
        public ProductsController(ProductDbContext context, AzureBlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        // PUBLIC ENDPOINTS
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] string? sort,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? tags = null,
            [FromQuery] string? gender = null
        )
        {
            var query = _context.Products.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search) || p.Tags.Contains(search));
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice);
            if (!string.IsNullOrEmpty(tags))
                query = query.Where(p => p.Tags.Contains(tags));
            if (!string.IsNullOrEmpty(gender))
                query = query.Join(_context.Categories.Where(c => c.Gender == gender), p => p.CategoryId, c => c.CategoryId, (p, c) => p);
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
            var result = products.Select(p => new ProductDto {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                Price = p.Price,
                Discount = p.Discount,
                Stock = p.Stock,
                ImageUrl = string.IsNullOrEmpty(p.ImageBlobName) ? null : _blobService.GetBlobSasUrl(p.ImageBlobName),
                Tags = p.Tags,
                IsFeatured = p.IsFeatured,
                IsNew = p.IsNew,
                IsOnSale = p.IsOnSale,
                CreatedAt = p.CreatedAt
            });
            return Ok(new { total, products = result });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            var result = new ProductDto {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                CategoryId = product.CategoryId,
                Price = product.Price,
                Discount = product.Discount,
                Stock = product.Stock,
                ImageUrl = string.IsNullOrEmpty(product.ImageBlobName) ? null : _blobService.GetBlobSasUrl(product.ImageBlobName),
                Tags = product.Tags,
                IsFeatured = product.IsFeatured,
                IsNew = product.IsNew,
                IsOnSale = product.IsOnSale,
                CreatedAt = product.CreatedAt
            };
            return Ok(result);
        }
// DTO for product responses
public class ProductDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public string Tags { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public bool IsOnSale { get; set; }
    public DateTime CreatedAt { get; set; }
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
        public async Task<IActionResult> GetByCategory(int categoryId)
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
            product.ImageBlobName = updated.ImageBlobName;
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
            var blobName = $"{id}{ext}";
            // Upload to Azure Blob Storage
            using (var stream = file.OpenReadStream())
            {
                var blobClient = _blobService.GetBlobClient(blobName);
                await blobClient.UploadAsync(stream, overwrite: true);
            }
            product.ImageBlobName = blobName;
            await _context.SaveChangesAsync();
            return Ok(_blobService.GetBlobSasUrl(blobName));
        }

        // No local image endpoint needed; images are served from Azure Blob Storage

        // CATEGORY ADMIN
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            // For demo: assign next available int ID
            category.CategoryId = _context.Categories.Any() ? _context.Categories.Max(c => c.CategoryId) + 1 : 1;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category updated)
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
