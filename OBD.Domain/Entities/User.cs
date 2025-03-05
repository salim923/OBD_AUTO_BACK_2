using Microsoft.AspNet.Identity.EntityFramework;

namespace OBD.Domain.Entities

{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string CIN { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
    public class getUser : IdentityUser
    {
        public string Username { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CIN { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class RegisterModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string CIN { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
    public class LoginModel
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
    public class UserUpdate
    {
        public required string Username { get; set; }
        public required string Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public required string Gender { get; set; }
        public required string Country { get; set; }
        public required string Address { get; set; }
        public required string CIN { get; set; }
    }

}