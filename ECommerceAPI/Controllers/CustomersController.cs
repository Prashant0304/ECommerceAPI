using ECommerceAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IUserService _userService;

        public CustomersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}/orders")]
        public async Task<IActionResult> GetCustomersOrders(int id)
        {
            var result = await _userService.GetCustomerOrders(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}
