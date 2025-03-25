using System;
using System.Collections.Generic;

namespace ParkIRC.Models
{
    public class ParkingRates
    {
        public int Id { get; set; }
        public string VehicleType { get; set; }
        public decimal BaseRate { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal DailyRate { get; set; }
        public decimal WeeklyRate { get; set; }
        public decimal MonthlyRate { get; set; }
        public TimeSpan GracePeriod { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 