using System;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Web.Models
{
    public class ParkingSpace
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public bool IsOccupied { get; set; }

        public string? VehicleNumber { get; set; }

        public DateTime? OccupiedSince { get; set; }

        public virtual Vehicle? Vehicle { get; set; }
    }
}