using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Product> _repository;
        private readonly AppDbContext _context;

        public ProductService(IGenericRepository<Product> repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllProducts( int pageNumber, int pageSize, string? search)
        {

            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if(!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            var products = await query
                .Skip((pageNumber -1)*pageSize)
                .Take(pageSize)
                .ToListAsync();

            return products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CategoryName =p.Category !=null ? p.Category.Name : null
            });
        }

        public async Task<ProductResponseDto?> GetProductById(int id)
        {

           

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return null;

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryName = product.Category?.Name
            };


        }

        public async Task<ProductResponseDto> CreateProduct(ProductCreateDto product)
        {
            var dto = new Product
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId
            };

            _context.Products.Add(dto);
            await _context.SaveChangesAsync();

            await _context.Entry(dto)
                .Reference(p => p.Category)
                .LoadAsync();


            return new ProductResponseDto
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                CategoryName = dto.Category?.Name
            };
        }

        public async Task<bool> UpdateProduct(int id, ProductCreateDto product)
        {
            var dto = await _context.Products.FindAsync(id);

            if(dto == null) 
                return false;

            dto.Name = product.Name;
            dto.Description = product.Description;
            dto.Price = product.Price;
            dto.Stock = product.Stock;
            dto.CategoryId = product.CategoryId;

            await _context.SaveChangesAsync();
            return true;
           
        }

        public async Task<bool> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
