using System;

namespace ParkIRC.Data.Models
{
    public class OccupancyData
    {
        public DateTime Date { get; set; }
        public int TotalSpaces { get; set; }
        public int OccupiedSpaces { get; set; }
        public int AvailableSpaces { get; set; }
        public double OccupancyRate { get; set; }
    }
}
