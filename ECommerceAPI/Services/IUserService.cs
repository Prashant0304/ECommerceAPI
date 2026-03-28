using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services
{
    public interface IUserService
    {
        Task<CustomerOrdersResponseDto?> GetCustomerOrders(int userId);
    }
}
