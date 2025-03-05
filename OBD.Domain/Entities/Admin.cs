using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBD.Domain.Entities
{
    public class Admin
    {
        public  int Id { get; set; }
        public  string Username { get; set; }
        public  string PasswordHash { get; set; }
        public  string Email { get; set; }


    }
    public class LoginModelAdmin
    {
        public  string Password { get; set; }
        public  string Email { get; set; }

    }
    public class RegisterModelAdmin
    {
        public  string Username { get; set; }
        public  string Password { get; set; }
        public  string Email { get; set; }

    }

}