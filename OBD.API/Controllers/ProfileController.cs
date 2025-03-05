using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBD.Domain.Entities;
using OBD.Infrastructure.Persistence;


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly UserManager<getUser> _userManager;
    private readonly ObdDbContext _context;

    public ProfileController(UserManager<getUser> userManager, ObdDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    //Get user profile information
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


    // Update user profile information
    [HttpPut("update")]
    public async Task<IActionResult> UpdateUser([FromBody] UserUpdate userUpdate)
    {
        if (userUpdate == null)
        {
            return BadRequest("Invalid input data.");
        }

        // Get email from the claims
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

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User updated successfully." });
    }

}