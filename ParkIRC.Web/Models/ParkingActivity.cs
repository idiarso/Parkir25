using System;

namespace ParkIRC.Models
{
    public class ParkingActivity
    {
        public string Id { get; set; } = string.Empty;
        public Vehicle Vehicle { get; set; } = new();
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string EntryGate { get; set; } = string.Empty;
        public string ExitGate { get; set; } = string.Empty;
        public string EntryOperator { get; set; } = string.Empty;
        public string ExitOperator { get; set; } = string.Empty;
        public int TotalItems { get; set; }
    }
}
