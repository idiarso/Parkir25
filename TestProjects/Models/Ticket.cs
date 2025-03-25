using System;

namespace ParkIRC.Test.Models
{
    public class Ticket
    {
        public string TicketNumber { get; set; } = string.Empty;
        public DateTime IssueTime { get; set; } = DateTime.Now;
        public string VehicleNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string ParkingSpace { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; } = DateTime.Now;
        public string Barcode { get; set; } = string.Empty;
    }
} 