using System;

namespace ParkIRC.Data.Models
{
    public class DashboardParkingActivity
    {
        public int TotalVehicles { get; set; }
        public int AvailableSpaces { get; set; }
        public int OccupiedSpaces { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
