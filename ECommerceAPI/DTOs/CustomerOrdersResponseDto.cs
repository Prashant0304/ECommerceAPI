namespace ECommerceAPI.DTOs
{
    public class CustomerOrdersResponseDto
    {
        public string CustomerEmail { get; set; } = string.Empty;
        public int TotalOrders {  get; set; }
        public decimal TotalSpent {  get; set; }
        public List<CustomerOrderDto> Orders { get; set; } = new();
    }
}
