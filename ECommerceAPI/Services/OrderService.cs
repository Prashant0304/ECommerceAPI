using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services
{
    public class OrderService:IOrderService
    {
        private readonly AppDbContext _context;
        private readonly EncryptionService _encryptionService;
        public OrderService(AppDbContext context,EncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        public async Task<IEnumerable<OrderResponseDto>> GetOrderAsync()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(o => new OrderResponseDto
            {
                Id = _encryptionService.Encrypt(o.Id.ToString()),
                OrderNumber = o.OrderNumber,
                OrderStatus = o.OrderStatus,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
            });
        }

        public async Task<int> CreatedOrderAsync(CreateOrderDto dto)
        {
            var productIds = dto.Items.Select(i => i.ProductId).ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            if (!products.Any())
                throw new Exception("Products not Found");

            decimal totalAmount = 0;

            var orderItems = new List<OrderItem>();

            foreach(var item in dto.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);

                var total = product.Price * item.Quantity;

                totalAmount += total;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price,
                    Total = total,
                });
            }

            var order = new Order
            {
                OrderNumber = Guid.NewGuid().ToString(),
                UserId = dto.UserId,
                TotalAmount = totalAmount,
                OrderStatus = "Pending",
                PaymentStatus = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();
            return order.Id;


        }


        public async Task<OrderResponseDto> GetOrderByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include( o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync( o => o.Id == id);

            if (order == null)
                throw new Exception("Order not Found");

            return new OrderResponseDto
            {
                Id = _encryptionService.Encrypt(order.Id.ToString()),
                OrderNumber = order.OrderNumber,
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                  
                Items = order.OrderItems.Select(i => new OrderItemDto
                {
                    ProductName = i.Product.Name,
                    ImageUrl = i.Product.ImageUrl,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Total = i.Total
                }).ToList()
            };
        }

        public async Task UpdatedOrdersStatusAsync(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                throw new Exception("Order not found");

            order.OrderStatus = status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        
        public async Task<OrderDetailsDto?> GetOrderDetailsAsync(int orderId)
        {
            var order = await _context.Orders
                .Include( o => o.User)
                .Include( o => o.OrderItems)
                .ThenInclude(oi =>oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if(order == null)
                return null;

            return new OrderDetailsDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                UserName = order.User?.Email ?? "",
                CreatedAt = order.CreatedAt,

                Items = order.OrderItems.Select(i => new OrderItemDetailsDto
                {
                    ProductName = i.Product.Name,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    Total = i.Total
                }).ToList()
            };
        }
    }
}
