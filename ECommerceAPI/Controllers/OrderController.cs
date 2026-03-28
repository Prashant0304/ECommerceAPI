using ECommerceAPI.DTOs;
using ECommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly EncryptionService _encryptionService;
        public OrderController(IOrderService orderService, EncryptionService encryptionService)
        {
            _orderService = orderService;
            _encryptionService = encryptionService;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _orderService.GetOrderAsync();

            var result = orders.Select(o => new
            {
                id = _encryptionService.Encrypt(o.Id.ToString()),
                orderNumber = o.OrderNumber,
                totalAmount =o.TotalAmount,
                orderStatus = o.OrderStatus,
                createdAt = o.CreatedAt
            });
            return Ok(orders);
        }

        
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (dto == null || dto.Items == null || !dto.Items.Any())
                return BadRequest(new { message = "Invalid order data" });

            var orderId = await _orderService.CreatedOrderAsync(dto);

            var encryptedOrderId = _encryptionService.Encrypt(orderId.ToString());
            
            return Ok(new
            {
                success = true,
                message = "Order created successfully",
                orderId = encryptedOrderId
            });
        }

        
        [HttpGet("{encryptedId}")]
        public async Task<IActionResult> GetOrderById(string encryptedId)
        
        {
            try
            {
                var decoded = Uri.UnescapeDataString(encryptedId);
                var decrypted =  _encryptionService.Decrypt(decoded);
                int id = int.Parse(decrypted);

                var order = await _orderService.GetOrderByIdAsync(id);
                return Ok(order);

            }
            catch (Exception ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        
        

        
        [HttpPut("{encryptedId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(
            string encryptedId,
            [FromQuery] string status)
        {
            if (string.IsNullOrEmpty(status))
                return BadRequest(new { message = "Status is required" });

            try
            {
                var decoded = Uri.UnescapeDataString(encryptedId);
                var decrypted = _encryptionService.Decrypt(decoded);
                int id = int.Parse(decrypted);

                await _orderService.UpdatedOrdersStatusAsync(id, status);

                return Ok(new
                {
                    success = true,
                    message = "Order status updated successfully"
                });
            }
            catch (Exception ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("details/{orderId}")]
        public async Task<IActionResult> GetOrdersDetails(string orderId)
        {
            Console.WriteLine("Encrypted Id received : " + orderId);

            int id = int.Parse(orderId);

            var order = await _orderService.GetOrderDetailsAsync(id);

            if(order == null)
                return NotFound();

            return Ok(order);
        }

    }
}