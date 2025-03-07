using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OBD.Infrastructure.Persistence;
using System.Linq;

namespace OBD.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AllCarsController : ControllerBase
    {
        private readonly ObdDbContext _context;
        public AllCarsController(ObdDbContext context)
        {
            _context = context;
        }

        [HttpGet("years")]
        public IActionResult GetYears()
        {
            var years = _context.OBD2.Select(c => c.Year).Distinct().OrderByDescending(y => y);
            return Ok(years);
        }

        [HttpGet("makes")]
        public IActionResult GetMakes([FromQuery] int year)
        {
            var makes = _context.OBD2
                .Where(c => c.Year == year)
                .Select(c => c.Make)
                .Distinct();
            return Ok(makes);
        }
        [HttpGet("models")]
        public IActionResult GetModels([FromQuery] int year, [FromQuery] string make)
        {
            var models = _context.OBD2
                .Where(c => c.Year == year&& c.Make == make)
                .Select(c => c.Model)
                .Distinct();
            return Ok(models);
        }

        [HttpGet("styles")]
        public IActionResult GetStyles([FromQuery] int year, [FromQuery] string make, [FromQuery] string model)
        {
            var styles = _context.OBD2
                .Where(c => c.Year == year && c.Make == make && c.Model == model)
                .SelectMany(c => c.Body_Styles)
                .Distinct();
            return Ok(styles);
        }


     

    }
}
