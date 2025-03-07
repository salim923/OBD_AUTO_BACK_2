using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBD.Domain.Entities
{
    public class Garage
    {
        public int GarageId { get; set; }
        public required string Name { get; set; }
        public required string Location { get; set; }
        public required string Contact { get; set; }

        public List<OpeningHour> OpeningHours { get; set; } = new List<OpeningHour>();
    }

    public class GarageInsert
    {
        public int GarageId { get; set; }
        public required string Name { get; set; }
        public required string Location { get; set; }
        public required string Contact { get; set; }

        public List<OpeningHourInsert> OpeningHours { get; set; } = new List<OpeningHourInsert>();
    }


    public class GarageUpdate
    {
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? Contact { get; set; }

        public List<OpeningHourInsert> OpeningHours { get; set; } = new List<OpeningHourInsert>();
    }



}