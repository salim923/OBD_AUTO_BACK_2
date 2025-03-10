﻿using Microsoft.AspNetCore.Identity;
namespace OBD.Domain.Entities
{
    public class Cars
    {
        public int Id { get; set; }
        public string Year { get; set; } 
        public string Make { get; set; }
        public string Model { get; set; }
        public List<string> Body_Style { get; set; }

        public int userId { get; set; }
        public User? User { get; set; }

        public float Millage { get; set; }


    }
    public class CarDto
    {
        public int Id { get; set; }
        public string Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public List<string> Body_Style { get; set; }
        public float Millage { get; set; }
    }

    public class CarInsertDto
    {
        public string Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public List<string> Body_Style { get; set; }
        public float Millage { get; set; }
    }



    public class CarUpdate
    {
      
        public float Millage { get; set; }
    }

    public class MaintenanceRequest
    {
        public string Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Millage { get; set; }
    }
}
