namespace ECommerceAPI.DTOs
{
    public class OrderDetailsDto
    {
        public int Id { get; set; }
        public string OrderNumber {  get; set; } = string.Empty;
        public decimal TotalAmount {  get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public string PaymentStatus {  get; set; } = string.Empty;
        public DateTime CreatedAt {  get; set; }
        public List<OrderItemDetailsDto> Items { get; set; } = new();


    }
}
