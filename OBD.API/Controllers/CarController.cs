using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBD.Domain.Entities;
using OBD.Infrastructure.Persistence;
using System.Security.Claims;

namespace OBD.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : ControllerBase
    {
        private readonly ObdDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly GeminiService _geminiService;

        public CarController(ObdDbContext context, IConfiguration configuration, GeminiService geminiService)
        {
            _context = context;
            _configuration = configuration;
            _geminiService = geminiService;
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return null;
            }
            return int.Parse(userIdClaim);
        }

        [HttpGet]
        [Route("GetCars")]
        public async Task<IActionResult> GetCarsByUserID()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            var cars = await _context.Cars
                .Where(c => c.User.Id == userId)
                .Select(c => new CarDto
                {
                    Id = c.Id,
                    Year = c.Year,
                    Make = c.Make,
                    Model = c.Model,
                    Body_Style = c.Body_Style,
                    Millage = c.Millage
                })
                .ToListAsync();
            return Ok(cars);
        }

        [HttpGet]
        [Route("GetCar/{carId}")]
        public async Task<IActionResult> GetCarById(int carId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            var car = await _context.Cars.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == carId);
            if (car == null)
            {
                return NotFound("Car not found.");
            }
            if (car.User.Id != userId)
            {
                return Unauthorized("You do not have permission to view this car.");
            }

            var carDto = new CarDto
            {
                Id = car.Id,
                Year = car.Year,
                Make = car.Make,
                Model = car.Model,
                Body_Style = car.Body_Style,
                Millage = car.Millage
            };

            return Ok(carDto);
        }


        [HttpPost]
        [Route("AddCar")]
        public async Task<IActionResult> AddCarsByUserID([FromBody] CarInsertDto model)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var car = new Cars
            {
                User = user,
                Year = model.Year,
                Make = model.Make,
                Model = model.Model,
                Body_Style = model.Body_Style,
                Millage = model.Millage
            };

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return Ok(car);
        }

        [HttpPut]
        [Route("UpdateCar/{carId}")]
        public async Task<IActionResult> UpdateCarInformation(int carId, [FromBody] CarUpdate model)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            var car = await _context.Cars.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == carId);
            if (car == null)
            {
                return NotFound("Car not found.");
            }
            if (car.User.Id != userId)
            {
                return Unauthorized("You do not have permission to update this car.");
            }

            car.Millage = model.Millage;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        [Route("DeleteCar/{carId}")]
        public async Task<IActionResult> DeleteCar(int carId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            var car = await _context.Cars.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == carId);
            if (car == null)
            {
                return NotFound("Car not found.");
            }
            if (car.User.Id != userId)
            {
                return Unauthorized("You do not have permission to delete this car.");
            }
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            return Ok("Car deleted successfully.");
        }

        [HttpPost]
        [Route("GetMaintenanceSuggestions")]
        public async Task<IActionResult> GetMaintenanceSuggestions([FromBody] MaintenanceRequest request)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            /* if (!user.IsPremium)
             {
                 return Forbid("Access denied. Premium subscription required.");
             }*/

            var result = await _geminiService.GenerateMaintenanceSchedule(request.Year, request.Make, request.Model, request.Millage);
            var suggestions = result.Split('\n').Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
            return Ok(new { suggestions });
        }
    }
}