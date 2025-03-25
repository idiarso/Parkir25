using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Data;
using ParkIRC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkIRC.Controllers
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
                    .Where(t => t.PaymentTime.Date == today)
                    .Select(t => t.TotalAmount)
                    .SumAsync(t => t);
                    
                var weeklyRevenue = await _context.ParkingTransactions
                    .Where(t => t.PaymentTime.Date >= weekStart && t.PaymentTime.Date <= today)
                    .Select(t => t.TotalAmount)
                    .SumAsync(t => t);
                    
                var monthlyRevenue = await _context.ParkingTransactions
                    .Where(t => t.PaymentTime.Date >= monthStart && t.PaymentTime.Date <= today)
                    .Select(t => t.TotalAmount)
                    .SumAsync(t => t);
                
                // Get hourly occupancy data
                var hourlyOccupancy = await GetHourlyOccupancyData();
                
                // Get vehicle distribution data
                var vehicleDistribution = await GetVehicleTypeDistribution();
                
                // Get recent activity
                var recentActivity = await GetRecentActivity();

                var dashboardData = new DashboardViewModel
                {
                    TotalSpaces = totalSpaces,
                    AvailableSpaces = availableSpaces,
                    DailyRevenue = dailyRevenue,
                    WeeklyRevenue = weeklyRevenue,
                    MonthlyRevenue = monthlyRevenue,
                    RecentActivity = recentActivity,
                    HourlyOccupancy = hourlyOccupancy,
                    VehicleDistribution = vehicleDistribution
                };
                
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                return View("Error", new ErrorViewModel 
                { 
                    Message = "Terjadi kesalahan saat memuat dashboard. Silakan coba lagi nanti.",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
        }

        private async Task<List<ParkingActivity>> GetRecentActivity()
        {
            return await _context.ParkingTransactions
                .Include(t => t.Vehicle)
                .Include(t => t.ParkingSpace)
                .Where(t => t.Vehicle != null)
                .OrderByDescending(t => t.EntryTime)
                .Take(10)
                .Select(t => new ParkingActivity
                {
                    VehicleType = t.Vehicle.VehicleType ?? "Unknown",
                    LicensePlate = t.Vehicle.VehicleNumber ?? "Unknown",
                    Timestamp = t.EntryTime,
                    ActionType = t.ExitTime != default(DateTime) ? "Exit" : "Entry",
                    Fee = t.TotalAmount,
                    ParkingType = t.ParkingSpace != null ? t.ParkingSpace.SpaceType ?? "Unknown" : "Unknown"
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