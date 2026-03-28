namespace ECommerceAPI.DTOs
{
    public class OrderResponseDto
    {
        public string Id { get; set; }
        public string OrderNumber {  get; set; }
        public decimal TotalAmount {  get; set; }
        public string OrderStatus {  get; set; }
        public DateTime CreatedAt {  get; set; }

        public List<OrderItemDto> Items { get; set; }

    }
}
