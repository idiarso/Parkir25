using System;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Data.Models
{
    public class ParkingSpace
    {
        public int Id { get; set; }
        
        [Required]
        public string SpaceNumber { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        [Required]
        public string VehicleType { get; set; }
        
        public DateTime? OccupiedSince { get; set; }
        
        public int? VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}
