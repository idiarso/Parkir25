using System;

namespace ParkIRC.Models
{
    public class VehicleEntryRequest
    {
        public string PlateNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string GateId { get; set; } = string.Empty;
        public string? PhotoPath { get; set; }
        public bool PrintTicket { get; set; } = true;
        public string? Notes { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string? OperatorId { get; set; }
        public DateTime EntryTime { get; set; } = DateTime.Now;
    }
} 