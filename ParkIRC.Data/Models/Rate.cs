using System;

namespace ParkIRC.Data.Models
{
    public class Rate
    {
        public int Id { get; set; }
        public string VehicleType { get; set; }
        public decimal BaseFee { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal DailyRate { get; set; }
        public decimal MonthlyRate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
