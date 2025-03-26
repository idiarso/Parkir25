using System;

namespace ParkIRC.Data.Models
{
    public class VehicleDistributionData
    {
        public DateTime Date { get; set; }
        public int TotalVehicles { get; set; }
        public int CarCount { get; set; }
        public int MotorcycleCount { get; set; }
        public int TruckCount { get; set; }
    }
}
