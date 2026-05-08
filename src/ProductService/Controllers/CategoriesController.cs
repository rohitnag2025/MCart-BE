using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ProductDbContext _context;
        public CategoriesController(ProductDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories.ToListAsync();
            // Build tree structure
            var lookup = categories.ToLookup(c => c.ParentId);
            List<object> BuildTree(int? parentId)
            {
                return lookup[parentId]
                    .Select(cat => new {
                        cat.CategoryId,
                        cat.Name,
                        cat.Gender,
                        Subcategories = BuildTree(cat.CategoryId)
                    }).ToList<object>();
            }
            return Ok(BuildTree(null));
        }
    }
}
