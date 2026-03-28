namespace ECommerceAPI.DTOs
{
    public class LoginResponseDto
    {
        public bool RequiresOtp {  get; set; }
        public string? AccessToken {  get; set; }
        public string? RefreshToken {  get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
