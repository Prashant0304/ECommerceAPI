using ECommerceAPI.Models;

namespace ECommerceAPI.Services
{
    public interface IOtpService
    {
        Task SendOtpAsync(User user);
        Task<bool> ValidateOtpAsync(User user, string otp);

        Task<bool> ResendOtpAsync(User user);
    }
}
