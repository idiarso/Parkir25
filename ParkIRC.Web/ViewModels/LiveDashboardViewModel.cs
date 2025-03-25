using System;
using System.Collections.Generic;
using ParkIRC.Models;
using Microsoft.EntityFrameworkCore;

namespace ParkIRC.Web.ViewModels
{
    public class LiveDashboardViewModel
    {
        public int TotalSpaces { get; set; }
        public int AvailableSpaces { get; set; }
        public int OccupiedSpaces { get; set; }
        public decimal OccupancyRate { get; set; }
        public decimal DailyRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public List<ParkingActivity> RecentActivity { get; set; } = new();
        public PaginatedList<ParkingActivity> RecentActivities { get; set; } = new(new List<ParkingActivity>(), 0, 1, 10);
        public List<OccupancyData> HourlyOccupancy { get; set; } = new();
        public List<VehicleDistributionData> VehicleDistribution { get; set; } = new();
        public List<ParkingSpaceViewModel> ParkingSpaces { get; set; } = new();
        public List<AlertMessage> Alerts { get; set; } = new();
        
        // Pagination properties
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class AlertMessage
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class VehicleDistributionItem
    {
        public string VehicleType { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalItems { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalItems = count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
} 