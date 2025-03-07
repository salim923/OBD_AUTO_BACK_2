using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBD.Domain.Entities;
using OBD.Infrastructure.Persistence;

namespace OBD.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GarageController : ControllerBase
    {
        private readonly ObdDbContext _context;
        private readonly IConfiguration _configuration;


        public GarageController(ObdDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetGarages")]
        public async Task<ActionResult<IEnumerable<Garage>>> GetGarages()
        {
            var garages = await _context.Garages
                .Include(g => g.OpeningHours)
                .ToListAsync();

            return Ok(garages);
        }

        // get garage by id
        [HttpGet]
        [Route("GetGarage/{id}")]
        public async Task<ActionResult<Garage>> GetGarage(int id)
        {
            var garage = await _context.Garages
                .Include(g => g.OpeningHours)
                .FirstOrDefaultAsync(g => g.GarageId == id);

            if (garage == null)
            {
                return NotFound();
            }

            return Ok(garage);
        }

        // add garage
        [HttpPost]
        [Route("AddGarage")]
        public async Task<ActionResult<Garage>> AddGarage(GarageInsert garageInsert)
        {
            if (garageInsert == null)
            {
                return BadRequest("Garage data is required.");
            }

            // Convert GarageInsert to Garage entity
            var garage = new Garage
            {
                Name = garageInsert.Name,
                Location = garageInsert.Location,
                Contact = garageInsert.Contact,
                OpeningHours = garageInsert.OpeningHours.Select(o => new OpeningHour
                {
                    Day = o.Day,
                    OpeningTime = o.OpeningTime,
                    ClosingTime = o.ClosingTime
                }).ToList()
            };

            _context.Garages.Add(garage);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGarage), new { id = garage.GarageId }, garage);
        }

        // update garage
        [HttpPut]
        [Route("UpdateGarage/{id}")]
        public async Task<ActionResult> UpdateGarage(int id, [FromBody] GarageUpdate garageUpdate)
        {
            var existingGarage = await _context.Garages
                .Include(g => g.OpeningHours)
                .FirstOrDefaultAsync(g => g.GarageId == id);

            if (existingGarage == null)
            {
                return NotFound("Garage not found.");
            }
            if (!string.IsNullOrEmpty(garageUpdate.Name))
            {
                existingGarage.Name = garageUpdate.Name;
            }
            if (!string.IsNullOrEmpty(garageUpdate.Location))
            {
                existingGarage.Location = garageUpdate.Location;
            }
            if (!string.IsNullOrEmpty(garageUpdate.Contact))
            {
                existingGarage.Contact = garageUpdate.Contact;
            }
            if (garageUpdate.OpeningHours != null && garageUpdate.OpeningHours.Any())
            {

                foreach (var newOpeningHour in garageUpdate.OpeningHours)
                {
                    var existingOpeningHour = existingGarage.OpeningHours
                        .FirstOrDefault(o => o.Day == newOpeningHour.Day);
                    if (existingOpeningHour != null)
                    {
                        existingOpeningHour.OpeningTime = newOpeningHour.OpeningTime;
                        existingOpeningHour.ClosingTime = newOpeningHour.ClosingTime;
                    }
                    else
                    {
                        var newOpeningHourEntity = new OpeningHour
                        {
                            Day = newOpeningHour.Day,
                            OpeningTime = newOpeningHour.OpeningTime,
                            ClosingTime = newOpeningHour.ClosingTime,
                            GarageId = existingGarage.GarageId
                        };

                        existingGarage.OpeningHours.Add(newOpeningHourEntity);
                    }
                }
            }
            _context.Garages.Update(existingGarage);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // delete garage
        [HttpDelete]
        [Route("DeleteGarage/{id}")]
        public async Task<ActionResult> DeleteGarage(int id)
        {
            var garage = await _context.Garages
                .Include(g => g.OpeningHours)
                .FirstOrDefaultAsync(g => g.GarageId == id);

            if (garage == null)
            {
                return NotFound("Garage not found.");
            }

            _context.OpeningHours.RemoveRange(garage.OpeningHours);
            _context.Garages.Remove(garage);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}