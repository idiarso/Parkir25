using System;

namespace ParkIRC.Shared.Models
{
    public class Vehicle
    {
        public string? PlateNumber { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public string? VehicleType { get; set; }
        public decimal? Fee { get; set; }
    }
}
