using ECommerceAPI.Data;
using ECommerceAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace ECommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IDistributedCache _redisCache;
        public ProductController(AppDbContext context, ILogger<ProductController> logger, IMemoryCache cache, IDistributedCache redisCache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
            _redisCache = redisCache;
        }

       
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            string cacheKey = "redis_productList";

            var cachedProducts = await _redisCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedProducts))
            {
                _logger.LogInformation("Products fetched from Redis cache");

                var products = JsonSerializer.Deserialize<List<Product>>(cachedProducts);
                return Ok(products);
            }

            _logger.LogInformation("Products not in Redis. Fetching from DB");

            var dbProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            var serializedProducts = JsonSerializer.Serialize(dbProducts);

            await _redisCache.SetStringAsync(cacheKey, serializedProducts, options);

            _logger.LogInformation("Products stored in Redis cache");

            return Ok(dbProducts);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            string cacheKey = $"product_{id}";

            if(!_cache.TryGetValue(cacheKey, out Product product))

            {
                _logger.LogInformation("Product {ProductId} not in cache . Fetching from DB", id);

                product = await _context.Products
               .Include(p => p.Category)
               .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

                if (product == null)
                {
                    _logger.LogWarning("Product not found with Id {ProductId}", id);
                    return NotFound();
                }

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                    _cache.Set(cacheKey, product, cacheOptions);

                _logger.LogInformation("Product {ProductId} stored in cache.", id);
            }
            else
            {
                _logger.LogInformation("Product {ProductId} fetched from CACHE.", id);
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] Product product)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid product data received");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating new product {ProductName}", product.Name);

            if (product.ImageFile != null)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Products");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);

                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }

                product.ImageUrl = "/Products/" + fileName;
            }

            product.CreatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _cache.Remove("productList");
            await _redisCache.RemoveAsync("redis_productList");

            _logger.LogInformation("Product created with Id {ProductId}. cache cleared", product.Id);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            if (id != product.Id)
            {
                _logger.LogWarning("Product Id mismatch during update");
                return BadRequest(new { message = "Product Id mismatch" });
            }

            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (existingProduct == null)
            {
                _logger.LogWarning("Product not found for update with Id {ProductId}", id);
                return NotFound();
            }

            _logger.LogInformation("Updating product with Id {ProductId}", id);

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.OriginalPrice = product.OriginalPrice;
            existingProduct.Stock = product.Stock;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.Rating = product.Rating;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _cache.Remove("productList");
            _cache.Remove($"product_{id}");

            await _redisCache.RemoveAsync("redis_productList");
            _logger.LogInformation("Product updated and cache cleared.");

            return Ok(existingProduct);
        }

      
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation("Deleting product with Id {ProductId}", id);

            var product = await _context.Products.FindAsync(id);

            if (product == null || product.IsDeleted)
            {
                _logger.LogWarning("Product not found for deletion with Id {ProductId}", id);
                return NotFound();
            }

            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _cache.Remove("productList");
            _cache.Remove($"product_{id}");

            await _redisCache.RemoveAsync("redis_productList");

            _logger.LogInformation("Product soft deleted with Id {ProductId}. Cache cleared", id);

            return Ok(new { message = "Product deleted successfully" });
        }
    }
}