using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkIRC.Models;
using ParkIRC.Web.ViewModels;
using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ParkIRC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                TotalParkingSpaces = await _context.ParkingSpaces.CountAsync(),
                OccupiedParkingSpaces = await _context.ParkingSpaces.CountAsync(p => p.IsOccupied),
                AvailableParkingSpaces = await _context.ParkingSpaces.CountAsync(p => !p.IsOccupied),
                OccupancyRate = (await _context.ParkingSpaces.CountAsync(p => p.IsOccupied)) * 100.0 / (await _context.ParkingSpaces.CountAsync()),
                TotalTransactionsToday = await _context.ParkingTransactions
                    .CountAsync(t => t.EntryTime.Date == DateTime.Today),
                ActiveTransactions = await _context.ParkingTransactions
                    .CountAsync(t => t.ExitTime == null),
                RecentEntries = await _context.ParkingTransactions
                    .Where(t => t.EntryTime.Date == DateTime.Today)
                    .OrderByDescending(t => t.EntryTime)
                    .Take(5)
                    .Select(t => new DashboardParkingActivity
                    {
                        VehicleType = t.Vehicle.VehicleType,
                        LicensePlate = t.Vehicle.VehicleNumber,
                        Timestamp = t.EntryTime,
                        ActionType = "Entry",
                        Fee = 0,
                        ParkingType = "Paid"
                    })
                    .ToListAsync(),
                RecentExits = await _context.ParkingTransactions
                    .Where(t => t.ExitTime.HasValue && t.ExitTime.Value.Date == DateTime.Today)
                    .OrderByDescending(t => t.ExitTime)
                    .Take(5)
                    .Select(t => new DashboardParkingActivity
                    {
                        VehicleType = t.Vehicle.VehicleType,
                        LicensePlate = t.Vehicle.VehicleNumber,
                        Timestamp = t.ExitTime.Value,
                        ActionType = "Exit",
                        Fee = t.TotalAmount,
                        ParkingType = "Paid"
                    })
                    .ToListAsync(),
                RecentActivity = await _context.ParkingTransactions
                    .OrderByDescending(t => t.ExitTime ?? t.EntryTime)
                    .Take(10)
                    .Select(t => new DashboardParkingActivity
                    {
                        VehicleType = t.Vehicle.VehicleType,
                        LicensePlate = t.Vehicle.VehicleNumber,
                        Timestamp = t.ExitTime ?? t.EntryTime,
                        ActionType = t.ExitTime.HasValue ? "Exit" : "Entry",
                        Fee = t.ExitTime.HasValue ? t.TotalAmount : 0,
                        ParkingType = "Paid"
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
