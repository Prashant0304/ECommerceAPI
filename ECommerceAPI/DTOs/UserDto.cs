namespace ECommerceAPI.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string Role { get; set; } = "Customer";

        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "Active";



    }
}
