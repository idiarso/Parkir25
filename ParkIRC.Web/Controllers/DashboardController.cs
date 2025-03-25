using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkIRC.Data;
using ParkIRC.Models;
using ParkIRC.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkIRC.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ApplicationDbContext context,
            ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var today = DateTime.Today;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var monthStart = new DateTime(today.Year, today.Month, 1);

                // Get data sequentially to avoid DbContext concurrency issues
                var totalSpaces = await _context.ParkingSpaces.CountAsync();
                var availableSpaces = await _context.ParkingSpaces.CountAsync(s => !s.IsOccupied);
                
                var dailyRevenue = await _context.ParkingTransactions
                    .Where(t => t.PaymentTime.HasValue && t.PaymentTime.Value.Date == today)
                    .Select(t => t.TotalAmount)
                    .SumAsync();
                    
                var weeklyRevenue = await _context.ParkingTransactions
                    .Where(t => t.PaymentTime.HasValue && t.PaymentTime.Value.Date >= weekStart && t.PaymentTime.Value.Date <= today)
                    .Select(t => t.TotalAmount)
                    .SumAsync();
                    
                var monthlyRevenue = await _context.ParkingTransactions
                    .Where(t => t.PaymentTime.HasValue && t.PaymentTime.Value.Date >= monthStart && t.PaymentTime.Value.Date <= today)
                    .Select(t => t.TotalAmount)
                    .SumAsync();
                
                // Get hourly occupancy data
                var hourlyOccupancy = await GetHourlyOccupancyData();
                
                // Get vehicle distribution data
                var vehicleDistribution = await GetVehicleTypeDistribution();
                
                // Get recent activity
                var recentActivity = await GetRecentActivity();

                var dashboardData = new DashboardViewModel
                {
                    TotalParkingSpaces = totalSpaces,
                    AvailableParkingSpaces = availableSpaces,
                    OccupiedParkingSpaces = totalSpaces - availableSpaces,
                    OccupancyRate = totalSpaces > 0 ? ((double)(totalSpaces - availableSpaces) / totalSpaces) * 100 : 0,
                    TotalTransactionsToday = await _context.ParkingTransactions
                        .CountAsync(t => t.EntryTime.Date == today),
                    DailyRevenue = dailyRevenue,
                    WeeklyRevenue = weeklyRevenue,
                    MonthlyRevenue = monthlyRevenue,
                    RecentActivity = recentActivity,
                    HourlyOccupancy = hourlyOccupancy,
                    VehicleDistribution = vehicleDistribution,
                    ActiveTransactions = (await GetActiveTransactions()).Count
                };
                
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                return View(new DashboardViewModel());
            }
        }

        private async Task<List<DashboardParkingActivity>> GetRecentActivity()
        {
            var today = DateTime.Today;
            var activities = await _context.ParkingTransactions
                .Where(t => t.EntryTime.Date == today)
                .OrderByDescending(t => t.EntryTime)
                .Take(10)
                .Select(t => new DashboardParkingActivity
                {
                    VehicleType = t.VehicleType,
                    LicensePlate = t.LicensePlate,
                    Timestamp = t.EntryTime,
                    ActionType = "Entry",
                    Fee = 0,
                    ParkingType = t.ParkingType,
                    VehicleNumber = t.VehicleNumber,
                    LastActivity = t.EntryTime
                })
                .ToListAsync();

            var exits = await _context.ParkingTransactions
                .Where(t => t.ExitTime.HasValue && t.ExitTime.Value.Date == today)
                .OrderByDescending(t => t.ExitTime)
                .Take(10)
                .Select(t => new DashboardParkingActivity
                {
                    VehicleType = t.VehicleType,
                    LicensePlate = t.LicensePlate,
                    Timestamp = t.ExitTime.Value,
                    ActionType = "Exit",
                    Fee = t.TotalAmount,
                    ParkingType = t.ParkingType,
                    VehicleNumber = t.VehicleNumber,
                    LastActivity = t.ExitTime
                })
                .ToListAsync();

            activities.AddRange(exits);
            return activities.OrderByDescending(a => a.Timestamp).Take(10).ToList();
        }

        private async Task<List<DashboardParkingActivity>> GetActiveTransactions()
        {
            return await _context.ParkingTransactions
                .Where(t => t.ExitTime == null)
                .OrderByDescending(t => t.EntryTime)
                .Select(t => new DashboardParkingActivity
                {
                    VehicleType = t.VehicleType,
                    LicensePlate = t.LicensePlate,
                    Timestamp = t.EntryTime,
                    ActionType = "Active",
                    Fee = 0,
                    ParkingType = t.ParkingType,
                    VehicleNumber = t.VehicleNumber,
                    LastActivity = t.EntryTime
                })
                .ToListAsync();
        }

        private async Task<List<OccupancyData>> GetHourlyOccupancyData()
        {
            var today = DateTime.Today;
            var totalSpaces = await _context.ParkingSpaces.CountAsync();
            
            // Initialize full day hours (6:00 - 22:00)
            var hourlyData = Enumerable.Range(6, 17).Select(hour => new OccupancyData 
            { 
                Hour = $"{hour:D2}:00",
                Count = 0,
                OccupancyPercentage = 0
            }).ToList();
            
            // Get actual data from database
            var dbHourlyData = await _context.ParkingTransactions
                .Where(t => t.EntryTime.Date == today)
                .GroupBy(t => t.EntryTime.Hour)
                .Select(g => new OccupancyData
                {
                    Hour = $"{g.Key:D2}:00",
                    Count = g.Count(),
                    OccupancyPercentage = totalSpaces > 0 ? (double)g.Count() / totalSpaces * 100 : 0
                })
                .ToListAsync();
            
            // Merge database data with initialized hours
            var comparer = new OccupancyDataComparer();
            return hourlyData.Select(h => 
                dbHourlyData.FirstOrDefault(d => comparer.Equals(d, h)) ?? h
            ).ToList();
        }

        private async Task<List<VehicleDistributionData>> GetVehicleTypeDistribution()
        {
            return await _context.Vehicles
                .Where(v => v.IsParked)
                .GroupBy(v => v.VehicleType)
                .Select(g => new VehicleDistributionData
                {
                    Type = g.Key ?? "Unknown",
                    Count = g.Count()
                })
                .ToListAsync();
        }
    }
} 