using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBD.Domain.Entities
{
    public class OpeningHour

    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdOpeningHour { get; set; }
        public string? Day { get; set; }
        public string? OpeningTime { get; set; }
        public string? ClosingTime { get; set; }
        public int GarageId { get; set; }
    }

    public class OpeningHourInsert
    {
        public string? Day { get; set; }
        public string? OpeningTime { get; set; }
        public string? ClosingTime { get; set; }
    }

    public class OpeningHourUpdate
    {
        public string? Day { get; set; }
        public string? OpeningTime { get; set; }
        public string? ClosingTime { get; set; }
    }

}