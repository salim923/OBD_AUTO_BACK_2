﻿
namespace OBD.Domain.Entities
{
    public class PasswordResetRequestModel
    {
        public required string Email { get; set; }
    }

    public class PasswordResetModel
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }

    
}
