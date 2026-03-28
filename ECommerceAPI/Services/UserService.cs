using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerOrdersResponseDto?> GetCustomerOrders(int userId)
        {
            var user = await _context.Users.Include(u => u.Orders).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return null;
            }
            var orders = user.Orders.Select( o => new CustomerOrderDto
            {
                OrderNumber = o.OrderNumber,
                TotalAmount = o.TotalAmount,
                OrderStatus = o.OrderStatus,
                CreatedAt = o.CreatedAt
            }).ToList();

            var response = new CustomerOrdersResponseDto
            {
                CustomerEmail = user.Email ?? "",
                TotalOrders = orders.Count,
                TotalSpent = orders.Sum(o => o.TotalAmount),
                Orders = orders,
            };
            return response;
        }
    }
}
