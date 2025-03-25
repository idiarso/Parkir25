using System;

namespace ParkIRC.Models
{
    public class EntryRequest
    {
        public string GateId { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public string OperatorId { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
    }
} 