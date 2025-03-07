using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBD.Domain.Entities;
using OBD.Infrastructure.Persistence;

namespace OBD.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ObdDbContext _context;

        public ProfileController(ObdDbContext context)
        {
            _context = context;
        }

       
        [HttpGet("user-info")]
        public async Task<IActionResult> GetProfile()
        {
            var emailClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email");
            var email = emailClaim?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(new { message = "Unauthorized - Email not found in token" });
            }

            var user = await _context.Users
                .Where(u => u.Email == email)
                .Select(u => new
                {
                    u.Username,
                    u.Email,
                    u.Phone,
                    u.Gender,
                    u.Country,
                    u.DateOfBirth,
                    u.Address,
                    u.CIN
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }

      
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] User userUpdate)
        {
            if (userUpdate == null)
            {
                return BadRequest("Invalid input data.");
            }

            var emailClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email");
            var email = emailClaim?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(new { message = "Unauthorized - Email not found in token" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Username = userUpdate.Username;
            user.Phone = userUpdate.Phone;
            user.DateOfBirth = userUpdate.DateOfBirth;
            user.Gender = userUpdate.Gender;
            user.Country = userUpdate.Country;
            user.CIN = userUpdate.CIN;
            user.Address = userUpdate.Address;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User updated successfully." });
        }
    }
}
