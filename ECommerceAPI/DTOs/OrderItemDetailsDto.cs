namespace ECommerceAPI.DTOs
{
    public class OrderItemDetailsDto
    {
        public string ProductName {  get; set; }
        public string ImageUrl {  get; set; }
        public int Quantity {  get; set; }
        public decimal Price { get; set; }
        public decimal Total {  get; set; }

    }
}
