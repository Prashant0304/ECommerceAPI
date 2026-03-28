using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDto>> GetAllProducts(int pageNumber,int pageSize, string? search);

        Task<ProductResponseDto?> GetProductById(int id);

        Task<ProductResponseDto> CreateProduct(ProductCreateDto productDto);

        Task<bool> UpdateProduct(int id, ProductCreateDto productDto);

        Task<bool> DeleteProduct(int id);
    }
}
