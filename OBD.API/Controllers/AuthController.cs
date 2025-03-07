using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OBD.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OBD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.Google;

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

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.Phone) ||
                string.IsNullOrWhiteSpace(model.CIN) ||
                string.IsNullOrWhiteSpace(model.Gender) ||
                string.IsNullOrWhiteSpace(model.Country) ||
                string.IsNullOrWhiteSpace(model.Address))
            {
                return BadRequest("All required fields must be provided.");
            }
            if (await _context.Users.AnyAsync(u => u.CIN == model.CIN))
                return BadRequest("CIN is already taken.");
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                return BadRequest("Email is already in use.");
            if (!Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest("Invalid email format.");
            if (!Regex.IsMatch(model.Password, @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,}$"))
                return BadRequest("Password must be at least 6 characters long and contain both letters and numbers.");
            if (model.Phone.Length < 8 || model.Phone.Length > 15)
                return BadRequest("Phone number must be between 8 and 15 characters.");
            if (model.Phone.Length < 8)
                return BadRequest("Cin number must be 8 at least characters.");

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Phone = model.Phone,
                CIN = model.CIN,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Country = model.Country,
                Address = model.Address
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }


        // Google login
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth", null, Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return BadRequest("Google authentication failed.");

            var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
                return BadRequest("Unable to retrieve user information.");

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser == null)
            {
                return BadRequest("User not found. Please sign up first.");
            }

            var token = GenerateJwtToken(existingUser);
            var redirectUrl = $"http://localhost:4200/auth-redirect?token={token}";
            return Redirect(redirectUrl);
        }

        // Sign up with Google
        [HttpGet("signup-google")]
        public IActionResult SignUpGoogleLogin()
        {
            var redirectUrl = Url.Action("SignUpGoogleResponse", "Auth", null, Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("signup-google-response")]
        public async Task<IActionResult> SignUpGoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return BadRequest("Google authentication failed.");

            var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
                return BadRequest("Unable to retrieve user information.");

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                return BadRequest("User already exists.");
            }

            var newUser = new User
            {
                Username = name,
                Email = email
            };


            var token = GenerateJwtToken(newUser);
            var redirectUrl = $"http://localhost:4200/user-profile?token={token}";
            return Redirect(redirectUrl);
        }

        //reset psw

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
        [HttpGet("GenerateJwtToken")]
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
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.PreferredUsername, user.Username),
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
