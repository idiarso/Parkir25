using System;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Data.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        
        [Required]
        public string PlateNumber { get; set; }
        
        [Required]
        public string VehicleType { get; set; }
        
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public decimal? Fee { get; set; }
        
        public int? ParkingSpaceId { get; set; }
        public ParkingSpace ParkingSpace { get; set; }
    }
}
