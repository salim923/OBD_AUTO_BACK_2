using Microsoft.AspNetCore.Mvc;
using OBD.Domain.Entities;
using OBD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using OBD.API.Controllers;

namespace OBD.API.Controllers
{
    [ApiController]
    [Route("api/auth/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly ObdDbContext _context;
        private readonly AuthController _authController;

        public RegisterController(ObdDbContext context, AuthController authController)
        {
            _context = context;
            _authController = authController;
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


       
        [HttpGet("signup-google")]
        public IActionResult SignUpGoogleLogin()
        {
            var redirectUrl = Url.Action("SignUpGoogleResponse", "GoogleAuth", null, Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            Console.WriteLine("signup google work!");
            return Challenge(properties, "GoogleSignUp");
        }
       

        [HttpGet("signup-google-response")]
        public async Task<IActionResult> SignUpGoogleResponse()
        {
            Console.WriteLine("signup google response work!");
            var authenticateResult = await HttpContext.AuthenticateAsync("GoogleSignUp");

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
            var token = _authController.GenerateJwtToken(newUser);
            var redirectUrl = $"http://localhost:4200/user-profile?token={token}";
            return Redirect(redirectUrl);
        }



    }
}
