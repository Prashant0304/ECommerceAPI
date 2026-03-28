namespace ECommerceAPI.DTOs
{
    public class CustomerOrderDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public DateTime CreatedAt {  get; set; }

    }
}
