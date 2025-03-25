using System;
using System.Collections.Generic;

namespace ParkIRC.Web.ViewModels
{
    public class DashboardParkingActivity
    {
        public string VehicleType { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public string ParkingType { get; set; } = string.Empty;
    }

    public class OccupancyData
    {
        public string Hour { get; set; } = string.Empty;
        public int Count { get; set; }
        public double OccupancyPercentage { get; set; }
    }

    public class VehicleDistributionData
    {
        public string Type { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class OccupancyDataComparer : IEqualityComparer<OccupancyData>
    {
        public bool Equals(OccupancyData? x, OccupancyData? y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;
            return x.Hour == y.Hour;
        }

        public int GetHashCode(OccupancyData obj)
        {
            return obj.Hour.GetHashCode();
        }
    }

    public class DashboardViewModel
    {
        // Parking Space Information
        public int TotalParkingSpaces { get; set; }
        public int AvailableParkingSpaces { get; set; }
        public int OccupiedParkingSpaces { get; set; }
        public double OccupancyRate { get; set; }

        // Transaction Information
        public int TotalTransactionsToday { get; set; }
        public int ActiveTransactions { get; set; }
        public decimal DailyRevenue { get; set; }
        public decimal WeeklyRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }

        // Activity Lists
        public List<DashboardParkingActivity> RecentEntries { get; set; } = new();
        public List<DashboardParkingActivity> RecentExits { get; set; } = new();
        public List<DashboardParkingActivity> RecentActivity { get; set; } = new();

        // Analytics
        public List<OccupancyData> HourlyOccupancy { get; set; } = new();
        public List<VehicleDistributionData> VehicleDistribution { get; set; } = new();
    }
} 