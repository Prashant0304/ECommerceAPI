using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDto>> GetOrderAsync();

        Task<OrderResponseDto> GetOrderByIdAsync(int id);
        Task<int> CreatedOrderAsync(CreateOrderDto dto);
        Task UpdatedOrdersStatusAsync(int id, string status);

        Task<OrderDetailsDto?> GetOrderDetailsAsync(int orderId);
     
    }
}
