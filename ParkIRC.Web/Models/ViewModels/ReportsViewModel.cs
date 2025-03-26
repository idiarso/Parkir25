using System;
using System.Collections.Generic;

namespace ParkIRC.Models.ViewModels
{
    public class ReportsViewModel
    {
        public List<ParkingTransaction> DailyTransactions { get; set; } = new List<ParkingTransaction>();
        public decimal MonthlyRevenue { get; set; }
        public List<VehicleTypeStats> VehicleTypeStats { get; set; } = new List<VehicleTypeStats>();
        public DateTime CurrentDate { get; set; } = DateTime.Now;
    }

    public class VehicleTypeStats
    {
        public string VehicleType { get; set; }
        public int Count { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageTransaction { get; set; }
    }
}
