using System;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Web.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Number { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        public DateTime EntryTime { get; set; }

        public DateTime? ExitTime { get; set; }

        public decimal? Amount { get; set; }

        public virtual ParkingSpace? ParkingSpace { get; set; }
    }
}