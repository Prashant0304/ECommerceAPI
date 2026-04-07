using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace ECommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IOtpService _otpService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(AppDbContext context , IConfiguration configuration,IOtpService otpService)
        {
            _context = context;
            _configuration = configuration;
            _otpService = otpService;
        }


        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(15),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.Trim().ToLower() == dto.Email.Trim().ToLower());

            if (existingUser != null)
                return BadRequest("User already exists");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email.Trim().ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Customer"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email.Trim().ToLower());

            if (user == null)
                return Unauthorized("Invalid email or password");

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!isValidPassword)
                return Unauthorized("Invalid email or password");
                
            await _otpService.SendOtpAsync(user);

            return Ok(new  LoginResponseDto
            {
                RequiresOtp = true,
                Message="OTP sent to your email"
            });
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);
            if (user == null)
                return Unauthorized("Invalid refresh Token");

            if(user.RefreshTokenExpiryTime <=DateTime.Now)
                return Unauthorized("Refresh Token expired");

            var newAccesstoken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken = newAccesstoken,
                refreshToken = newRefreshToken
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(TokenRequestDto dto) {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);
            if (user == null) return Unauthorized("Invalid refresh token");
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _context.SaveChangesAsync();
            return Ok("logged out succefully");
        }


        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                UserId = userId,
                Email = email,
                Role = role
            });
        }


        [HttpPost("verify-otp")]
        public async Task<ActionResult<LoginResponseDto>> VerifyOtp(VerifyOtpDto dto)
        {
            var user = await _context.Users.
                FirstOrDefaultAsync(u => u.Email == dto.Email.Trim().ToLower());

            if (user == null)
                return BadRequest("user not found");
            var isValid = await _otpService.ValidateOtpAsync(user, dto.Otp);

            if (!isValid) return BadRequest("Invalid or expired OTP");

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

            await _context.SaveChangesAsync();
            return Ok(new LoginResponseDto
            {
                RequiresOtp = false,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Message = "Login Successfull"
            });

        }


        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(
                u=> u.Email == dto.Email.Trim().ToLower());

            if(user == null)
                return BadRequest("User Not Found");

            await _otpService.ResendOtpAsync(user);
            return Ok("Otp resent successfully");
        }
    }
}
