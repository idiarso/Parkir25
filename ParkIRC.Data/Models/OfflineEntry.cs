using System;

namespace ParkIRC.Data.Models
{
    public class OfflineEntry
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; }
        public string VehicleType { get; set; }
        public DateTime EntryTime { get; set; }
        public string EntryGate { get; set; }
        public string Operator { get; set; }
    }
}
