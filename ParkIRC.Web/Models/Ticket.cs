using System;

namespace ParkIRC.Models
{
    public class Ticket
    {
        public string TicketNumber { get; set; } = string.Empty;
        
        public string VehicleNumber { get; set; } = string.Empty;
        
        public string VehicleType { get; set; } = string.Empty;
        
        public string ParkingSpace { get; set; } = string.Empty;
        
        public DateTime IssueTime { get; set; } = DateTime.Now;
        
        public DateTime EntryTime { get; set; } = DateTime.Now;
        
        public string Barcode { get; set; } = string.Empty;
        
        public override string ToString()
        {
            return $"Ticket: {TicketNumber}, Vehicle: {VehicleNumber}, Type: {VehicleType}, Space: {ParkingSpace}";
        }
    }
} 