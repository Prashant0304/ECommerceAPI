using ECommerceAPI.Data;
using ECommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services
{
    public class OtpService :IOtpService
    {
        private readonly AppDbContext _context;

        public OtpService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SendOtpAsync(User user)
        {
            var otp = new Random().Next(100000, 999999).ToString();

            var userOtp = new UserOtp
            {
                UserId = user.Id,
                OtpCode = otp,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsValidated = false
            };

            _context.UserOtps.Add(userOtp);
            await _context.SaveChangesAsync();

            Console.WriteLine($"OTP for {user.Email}: {otp}");
        }

        public async Task<bool> ValidateOtpAsync(User user, string otp)
        {
            otp = otp.Trim();
            var now = DateTime.UtcNow;

            var userOtp = await _context.UserOtps
                .Where(x => x.UserId == user.Id &&
                            x.OtpCode == otp &&
                            !x.IsValidated &&
                            x.IsActive &&
                            x.ExpiryTime > now)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (userOtp == null)
                return false;

            userOtp.IsValidated = true;
            userOtp.IsActive = false;   

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ResendOtpAsync(User user)
        {
            var now = DateTime.UtcNow;

            var activeOtps = await _context.UserOtps
                .Where(x => x.UserId == user.Id &&
                x.IsActive && !x.IsValidated).ToListAsync();

            foreach(var otp in activeOtps)
            {
                otp.IsActive = false;
            }

            var newOtpCode = new Random().Next(100000, 999999).ToString();

            var newOtp = new UserOtp
            {
                UserId = user.Id,
                OtpCode = newOtpCode,
                CreatedAt = now,
                ExpiryTime = now.AddMinutes(5),
                IsValidated = false,
                IsActive = true
            };
            _context.UserOtps.Add(newOtp);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Resent OTP for {user.Email}: {newOtpCode}");
            return true;
        }
    }
}
