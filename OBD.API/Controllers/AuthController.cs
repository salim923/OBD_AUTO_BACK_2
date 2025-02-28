using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OBD.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OBD.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Net.Mail;
using System.Net;

namespace OBD.API.Controllers
{
    [ApiController]
    [Route("api/jwtauth/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ObdDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(ObdDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        // Logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out successfully" });
        }



        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
                return NotFound("User not found.");

            var token = GeneratePasswordResetToken(user);
            // Send token to user's email (implementation depends on your email service)
            SendPasswordResetEmail(user.Email, token);

            return Ok(new { message = "Password reset email sent." });
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
                return NotFound("User not found.");

            if (!ValidatePasswordResetToken(user, model.Token))
                return BadRequest("Invalid or expired token.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successfully." });
        }

        // JWT Token Generation
        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"];

            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException("JWT Secret is not configured properly.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GeneratePasswordResetToken(User user)
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        private bool ValidatePasswordResetToken(User user, string token)
        {
           
            return true;
        }

        private void SendPasswordResetEmail(string email, string token)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var smtpClient = new SmtpClient(smtpSettings["Host"])
            {
                Port = int.Parse(smtpSettings["Port"]),
                Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                EnableSsl = true,
            };

            var resetLink = $"{_configuration["AppSettings:ClientUrl"]}/reset-password?token={token}&email={email}";

            var mailMessage = new MailMessage
            {
                From = new MailAddress("ferchichisalim599@gmail.com"),
                Subject = "Password Reset",
                Body = $"Please use the following link to reset your password: <a href='{resetLink}'>Reset Password</a>",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            smtpClient.Send(mailMessage);
        }


    }
}
