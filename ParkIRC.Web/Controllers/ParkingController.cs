using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Models;
using ParkIRC.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using ParkIRC.Hubs;
using ParkIRC.Services;
using Microsoft.AspNetCore.Authorization;
using ParkIRC.Extensions;
using System.Text.Json;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ParkIRC.Web.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using ParkIRC.Web.Extensions;

namespace ParkIRC.Controllers
{
    [Authorize]
    public class ParkingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ParkingController> _logger;
        private readonly IHubContext<ParkingHub> _hubContext;
        private readonly IParkingService _parkingService;
        private readonly PrintService _printService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ParkingController(
            ApplicationDbContext context,
            ILogger<ParkingController> logger,
            IParkingService parkingService,
            IHubContext<ParkingHub> hubContext,
            PrintService printService,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _parkingService = parkingService;
            _hubContext = hubContext;
            _printService = printService;
            _webHostEnvironment = webHostEnvironment;
        }
        
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> Dashboard()
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
                
                // Get data that requires more complex queries
                var recentActivity = await GetRecentActivity();
                var hourlyOccupancy = await GetHourlyOccupancyData();
                var vehicleDistribution = await GetVehicleTypeDistribution();

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

        private async Task<List<DashboardParkingActivity>> GetActiveTransactions()
        {
            return await _context.ParkingTransactions
                .Include(t => t.Vehicle)
                .Where(t => !t.ExitTime.HasValue)
                .OrderByDescending(t => t.EntryTime)
                .Take(10)
                .Select(t => new DashboardParkingActivity
                {
                    VehicleType = t.Vehicle.VehicleType,
                    LicensePlate = t.Vehicle.VehicleNumber,
                    Timestamp = t.EntryTime,
                    ActionType = "Active",
                    Fee = t.TotalAmount,
                    ParkingType = t.ParkingSpace.Type
                })
                .ToListAsync();
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

        private async Task<List<OccupancyData>> GetHourlyOccupancyData()
        {
            var today = DateTime.Today;
            
            // Get the total spaces count first
            var totalSpaces = await _context.ParkingSpaces.CountAsync();
            
            // Then get the hourly data with the total spaces value
            var hourlyData = await _context.ParkingTransactions
                .Where(t => t.EntryTime.Date == today)
                .GroupBy(t => t.EntryTime.Hour)
                .Select(g => new OccupancyData
                {
                    Hour = $"{g.Key:D2}:00",
                    Count = g.Count(),
                    OccupancyPercentage = totalSpaces > 0 ? (double)g.Count() / totalSpaces * 100 : 0
                })
                .ToListAsync();

            // Fill in missing hours with zero values
            var allHours = Enumerable.Range(0, 24)
                .Select(h => new OccupancyData
                {
                    Hour = $"{h:D2}:00",
                    Count = 0,
                    OccupancyPercentage = 0
                }).ToList();
            
            // Merge the actual data with the zero-filled hours
            foreach (var data in hourlyData)
            {
                var hour = int.Parse(data.Hour.Split(':')[0]);
                allHours[hour] = data;
            }

            return allHours.OrderBy(x => x.Hour).ToList();
        }

        private async Task<List<VehicleDistributionData>> GetVehicleTypeDistribution()
        {
            return await _context.Vehicles
                    .Where(v => v.IsParked)
                .GroupBy(v => v.VehicleType ?? "Unknown")
                .Select(g => new VehicleDistributionData
                    {
                        Type = g.Key,
                        Count = g.Count()
                    })
                .ToListAsync();
        }

        public IActionResult VehicleEntry()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VehicleEntry(VehicleEntryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Form tidak valid" });
            }

            try
            {
                var vehicle = new Vehicle
                {
                    VehicleNumber = model.VehicleNumber,
                    VehicleType = model.VehicleType,
                    DriverName = model.DriverName,
                    ContactNumber = model.ContactNumber,
                    IsParked = true,
                    EntryTime = DateTime.UtcNow
                };

                // Assign parking space
                var parkingSpace = await _parkingService.AssignParkingSpace(vehicle);
                if (parkingSpace == null)
                {
                    return Json(new { success = false, message = "Tidak ada tempat parkir tersedia" });
                }

                vehicle.ParkingSpaceId = parkingSpace.Id;
                vehicle.ParkingSpace = parkingSpace;

                // Save changes
                await _context.Vehicles.AddAsync(vehicle);
                await _context.SaveChangesAsync();

                // Update dashboard
                await _hubContext.Clients.All.SendAsync("UpdateDashboard");

                return Json(new { success = true, message = "Kendaraan berhasil dicatat" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing vehicle entry");
                return Json(new { success = false, message = "Terjadi kesalahan saat mencatat kendaraan" });
            }
        }

        private async Task<ParkingSpace?> FindOptimalParkingSpace(string vehicleType)
        {
            try
            {
                // Get all available spaces that match the vehicle type
                var availableSpaces = await _context.ParkingSpaces
                    .Where(s => !s.IsOccupied && s.SpaceType == vehicleType)
                    .ToListAsync();

                if (!availableSpaces.Any())
                {
                    _logger.LogWarning("No available parking spaces for vehicle type: {VehicleType}", vehicleType);
                    return null;
                }

                // For now, we'll use a simple strategy: pick the first available space
                // TODO: Implement more sophisticated space selection based on:
                // 1. Proximity to entrance/exit
                // 2. Space size optimization
                // 3. Traffic flow optimization
                return availableSpaces.First();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding optimal parking space for vehicle type: {VehicleType}", vehicleType);
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordEntry([FromBody] VehicleEntryModel entryModel)
        {
            if (entryModel == null)
            {
                _logger.LogWarning("Vehicle entry model is null");
                return BadRequest(new { error = "Data kendaraan tidak valid" });
            }

            try
            {
                _logger.LogInformation("Processing vehicle entry for {VehicleNumber}, Type: {VehicleType}", 
                    entryModel.VehicleNumber, entryModel.VehicleType);
                    
                // Validate the model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(kvp => kvp.Value != null && kvp.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        );
                    
                    _logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", errors.Values.SelectMany(v => v)));
                    return BadRequest(new { errors });
                }

                // Normalize vehicle number format
                entryModel.VehicleNumber = entryModel.VehicleNumber.ToUpper().Trim();
                
                // Check vehicle format separately
                var vehicleNumberRegex = new System.Text.RegularExpressions.Regex(@"^[A-Z]{1,2}\s\d{1,4}\s[A-Z]{1,3}$");
                if (!vehicleNumberRegex.IsMatch(entryModel.VehicleNumber))
                {
                    _logger.LogWarning("Invalid vehicle number format: {VehicleNumber}", entryModel.VehicleNumber);
                    return BadRequest(new { error = "Format nomor kendaraan tidak valid. Contoh: B 1234 ABC" });
                }

                // Check if vehicle already exists and is parked
                var existingVehicle = await _context.Vehicles
                    .Include(v => v.ParkingSpace)
                    .FirstOrDefaultAsync(v => v.VehicleNumber == entryModel.VehicleNumber);
                
                if (existingVehicle != null && existingVehicle.IsParked)
                {
                    _logger.LogWarning("Vehicle {VehicleNumber} is already parked", entryModel.VehicleNumber);
                    return BadRequest(new { error = "Kendaraan sudah terparkir" });
                }

                // Find optimal parking space automatically
                var optimalSpace = await FindOptimalParkingSpace(entryModel.VehicleType);
                if (optimalSpace == null)
                {
                    _logger.LogWarning("No available parking space for vehicle type: {VehicleType}", entryModel.VehicleType);
                    return BadRequest(new { error = $"Tidak ada ruang parkir tersedia untuk kendaraan tipe {GetVehicleTypeName(entryModel.VehicleType)}" });
                }

                // Create or update vehicle record
                if (existingVehicle == null)
                {
                    existingVehicle = new Vehicle
                    {
                        VehicleNumber = entryModel.VehicleNumber,
                        VehicleType = entryModel.VehicleType,
                        DriverName = entryModel.DriverName,
                        PhoneNumber = entryModel.PhoneNumber,
                        IsParked = true,
                        EntryTime = DateTime.Now,
                        ParkingSpace = optimalSpace
                    };
                    _context.Vehicles.Add(existingVehicle);
                }
                else
                {
                    existingVehicle.VehicleType = entryModel.VehicleType;
                    existingVehicle.DriverName = entryModel.DriverName;
                    existingVehicle.PhoneNumber = entryModel.PhoneNumber;
                    existingVehicle.IsParked = true;
                    existingVehicle.ParkingSpace = optimalSpace;
                }

                // Create parking transaction
                var transaction = new ParkingTransaction
                {
                    Vehicle = existingVehicle,
                    EntryTime = DateTime.Now,
                        TransactionNumber = GenerateTransactionNumber(),
                    Status = "Active"
                    };
                _context.ParkingTransactions.Add(transaction);

                // Update parking space status
                optimalSpace.IsOccupied = true;
                optimalSpace.LastOccupiedTime = DateTime.Now;

                    await _context.SaveChangesAsync();
                    
                // Notify clients about the update via SignalR
                    if (_hubContext != null)
                {
                    await _hubContext.Clients.All.SendAsync("UpdateParkingStatus", new
                    {
                        Action = "Entry",
                        VehicleNumber = entryModel.VehicleNumber,
                        SpaceNumber = optimalSpace.SpaceNumber,
                        SpaceType = optimalSpace.SpaceType,
                        Timestamp = DateTime.Now
                    });
                }

                _logger.LogInformation("Vehicle {VehicleNumber} entered the parking lot.", entryModel.VehicleNumber);

                return Ok(new
                {
                    message = "Kendaraan berhasil masuk",
                    spaceNumber = optimalSpace.SpaceNumber,
                    spaceType = optimalSpace.SpaceType,
                    transactionNumber = transaction.TransactionNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing vehicle entry for {VehicleNumber}", entryModel.VehicleNumber);
                return StatusCode(500, new { error = "Terjadi kesalahan saat memproses masuk kendaraan" });
            }
        }

        private string GetVehicleTypeName(string type)
        {
            return type.ToLower() switch
            {
                "car" => "mobil",
                "motorcycle" => "motor",
                "truck" => "truk",
                _ => type
            };
        }

        private async Task<object> GetHourlyOccupancy()
        {
            var today = DateTime.Today;
            var hourlyData = await _context.ParkingTransactions
                .Where(t => t.EntryTime.Date == today)
                .GroupBy(t => t.EntryTime.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Hour)
                .ToListAsync();

            return new
            {
                Labels = hourlyData.Select(x => $"{x.Hour}:00").ToList(),
                Data = hourlyData.Select(x => x.Count).ToList()
            };
        }

        private async Task<object> GetVehicleDistributionForDashboard()
        {
            var distribution = await GetVehicleTypeDistribution();
            return new
            {
                Labels = distribution.Select(x => x.Type).ToList(),
                Data = distribution.Select(x => x.Count).ToList()
            };
        }

        private async Task<List<object>> GetRecentParkingActivity()
        {
            var activities = await _context.ParkingTransactions
                .Where(t => t.Vehicle != null)
                .OrderByDescending(t => t.EntryTime)
                .Take(10)
                .Select(t => new ParkingActivityViewModel
                {
                    VehicleNumber = t.Vehicle != null ? t.Vehicle.VehicleNumber : "Unknown",
                    EntryTime = t.EntryTime,
                    ExitTime = t.ExitTime,
                    Duration = t.ExitTime.HasValue ? $"{(decimal)(t.ExitTime.Value - t.EntryTime).TotalHours:F1} jam" : "-",
                    Amount = t.TotalAmount,
                    Status = t.ExitTime != default(DateTime) ? "Completed" : "In Progress"
                })
                .ToListAsync();
                
            return activities.Cast<object>().ToList();
        }

        public IActionResult VehicleExit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessExit([FromBody] string vehicleNumber)
        {
            if (string.IsNullOrEmpty(vehicleNumber))
                return BadRequest(new { error = "Nomor kendaraan harus diisi." });

            try
            {
                // Normalize vehicle number
                vehicleNumber = vehicleNumber.ToUpper().Trim();

                // Find the vehicle and include necessary related data
                var vehicle = await _context.Vehicles
                    .Include(v => v.ParkingSpace)
                    .FirstOrDefaultAsync(v => v.VehicleNumber == vehicleNumber && v.IsParked);

                if (vehicle == null)
                    return NotFound(new { error = "Kendaraan tidak ditemukan atau sudah keluar dari parkir." });

                // Find active transaction
                var transaction = await _context.ParkingTransactions
                    .Where(t => t.VehicleId == vehicle.Id && t.ExitTime == default(DateTime))
                    .OrderByDescending(t => t.EntryTime)
                    .FirstOrDefaultAsync();

                if (transaction == null)
                    return NotFound(new { error = "Tidak ditemukan transaksi parkir yang aktif untuk kendaraan ini." });

                // Get applicable parking rate
                var parkingRate = await _context.Set<ParkingRateConfiguration>()
                    .Where(r => r.VehicleType == vehicle.VehicleType 
                            && r.IsActive 
                            && r.EffectiveFrom <= DateTime.Now 
                            && (!r.EffectiveTo.HasValue || r.EffectiveTo >= DateTime.Now))
                    .OrderByDescending(r => r.EffectiveFrom)
                    .FirstOrDefaultAsync();

                if (parkingRate == null)
                    return BadRequest(new { error = "Tidak dapat menemukan tarif parkir yang sesuai." });

                // Calculate duration and fee
                var exitTime = DateTime.Now;
                var duration = exitTime - transaction.EntryTime;
                var hours = Math.Ceiling(duration.TotalHours);
                
                decimal totalAmount;
                if (hours <= 1)
                {
                    totalAmount = parkingRate.BaseRate;
                }
                else if (hours <= 24)
                {
                    totalAmount = parkingRate.BaseRate + (parkingRate.HourlyRate * (decimal)(hours - 1));
                }
                else if (hours <= 168) // 7 days
                {
                    var days = Math.Ceiling(hours / 24);
                    totalAmount = parkingRate.DailyRate * (decimal)days;
                }
                else // more than 7 days
                {
                    var weeks = Math.Ceiling(hours / 168);
                    totalAmount = parkingRate.WeeklyRate * (decimal)weeks;
                }

                // Update transaction
                transaction.ExitTime = exitTime;
                transaction.TotalAmount = totalAmount;
                transaction.Status = "Completed";
                _context.ParkingTransactions.Update(transaction);

                // Update vehicle and parking space
                vehicle.ExitTime = exitTime;
                vehicle.IsParked = false;
                if (vehicle.ParkingSpace != null)
                {
                    vehicle.ParkingSpace.IsOccupied = false;
                    vehicle.ParkingSpace.LastOccupiedTime = exitTime;
                }
                _context.Vehicles.Update(vehicle);

                await _context.SaveChangesAsync();
                
                // Create response data
                var response = new
                {
                    message = "Kendaraan berhasil keluar",
                    vehicleNumber = vehicle.VehicleNumber,
                    entryTime = transaction.EntryTime,
                    exitTime = exitTime,
                    duration = $"{hours:0} jam",
                    totalAmount = totalAmount,
                    spaceNumber = vehicle.ParkingSpace?.SpaceNumber
                };

                // Notify clients if SignalR hub is available
                if (_hubContext != null)
                {
                    await _hubContext.Clients.All.SendAsync("UpdateParkingStatus", new
                    {
                        Action = "Exit",
                        VehicleNumber = vehicle.VehicleNumber,
                        SpaceNumber = vehicle.ParkingSpace?.SpaceNumber,
                        Timestamp = exitTime
                    });

                    var dashboardData = await GetDashboardData();
                    await _hubContext.Clients.All.SendAsync("ReceiveDashboardUpdate", dashboardData);
                }

                _logger.LogInformation("Vehicle {VehicleNumber} exited the parking lot.", vehicleNumber);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing vehicle exit for {VehicleNumber}", vehicleNumber);
                return StatusCode(500, new { error = "Terjadi kesalahan saat memproses keluar kendaraan." });
            }
        }

        private async Task<ParkingSpace?> GetAvailableParkingSpace(string type)
        {
            return await _context.ParkingSpaces
                .Where(ps => !ps.IsOccupied && ps.SpaceType.ToLower() == type.ToLower())
                .FirstOrDefaultAsync();
        }

        private decimal CalculateParkingFee(TimeSpan? duration, decimal hourlyRate)
        {
            if (duration == null || !duration.HasValue)
                return 0;

            var totalHours = (decimal)Math.Ceiling(duration.Value.TotalHours);
            return hourlyRate * totalHours;
        }

        private static string GenerateTransactionNumber()
        {
            return $"TRX-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}".ToUpper();
        }

        // Get available parking spaces by vehicle type
        [HttpGet]
        public async Task<IActionResult> GetAvailableSpaces()
        {
            try
            {
                var spaces = await _context.ParkingSpaces
                    .Where(ps => !ps.IsOccupied)
                    .GroupBy(ps => ps.SpaceType.ToLower())
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Type, x => x.Count);

                return Json(new
                {
                    car = spaces.GetValueOrDefault("car", 0),
                    motorcycle = spaces.GetValueOrDefault("motorcycle", 0),
                    truck = spaces.GetValueOrDefault("truck", 0)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available parking spaces");
                return StatusCode(500, new { error = "Gagal mengambil data slot parkir" });
            }
        }

        public IActionResult Reports()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }

        // Add missing GetDashboardData method
        private async Task<DashboardData> GetDashboardData()
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

            return new DashboardData
            {
                TotalSpaces = totalSpaces,
                AvailableSpaces = availableSpaces,
                OccupiedSpaces = totalSpaces - availableSpaces,
                DailyRevenue = dailyRevenue,
                WeeklyRevenue = weeklyRevenue,
                MonthlyRevenue = monthlyRevenue,
                TodayTransactions = await _context.ParkingTransactions
                    .CountAsync(t => t.EntryTime.Date == today)
            };
        }

        // Define a strongly-typed class for dashboard data
        public class DashboardData
        {
            public int TotalSpaces { get; set; }
            public int AvailableSpaces { get; set; }
            public int OccupiedSpaces { get; set; }
            public decimal DailyRevenue { get; set; }
            public decimal WeeklyRevenue { get; set; }
            public decimal MonthlyRevenue { get; set; }
            public int TodayTransactions { get; set; }
        }

        // New method for exporting dashboard data
        public async Task<IActionResult> ExportDashboardData()
        {
            try
            {
                var dashboardData = await GetDashboardData();
                // In a real implementation, you would use a library like EPPlus to create an Excel file
                // For now, we'll return a CSV
                var csv = "TotalSpaces,AvailableSpaces,DailyRevenue,WeeklyRevenue,MonthlyRevenue\n";
                csv += $"{dashboardData.TotalSpaces},{dashboardData.AvailableSpaces},{dashboardData.DailyRevenue},{dashboardData.WeeklyRevenue},{dashboardData.MonthlyRevenue}";
                
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "dashboard-report.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting dashboard data");
                return StatusCode(500, "Error exporting dashboard data");
            }
        }

        public async Task<IActionResult> GetParkingTransactions(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ParkingTransactions.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(t => t.EntryTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.ExitTime <= endDate.Value);
            }

            var transactions = await query.ToListAsync();
            return Json(transactions);
        }

        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                startDate = startDate.GetValueOrDefault(DateTime.Today);
                endDate = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var transactions = await _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .Include(t => t.ParkingSpace)
                    .Where(t => t.EntryTime >= startDate && t.EntryTime <= endDate)
                    .OrderByDescending(t => t.EntryTime)
                    .ToListAsync();

                var model = new HistoryViewModel
                {
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                    Transactions = transactions.Select(t => new TransactionHistoryItem
                    {
                        Id = t.Id.ToString(),
                        TransactionNumber = t.TransactionNumber,
                        TicketNumber = t.TicketNumber,
                        VehicleNumber = t.Vehicle.VehicleNumber,
                        VehicleType = t.Vehicle.VehicleType,
                        PlateNumber = t.Vehicle.VehicleNumber,
                        EntryTime = t.EntryTime,
                        ExitTime = t.ExitTime,
                        Duration = t.ExitTime.HasValue ? $"{(decimal)(t.ExitTime.Value - t.EntryTime).TotalHours:F1} jam" : "-",
                        Amount = t.TotalAmount,
                        PaymentStatus = t.PaymentStatus,
                        PaymentMethod = t.PaymentMethod,
                        PaymentTime = t.PaymentTime,
                        EntryGate = t.EntryPoint,
                        ExitGate = t.ExitPoint,
                        EntryOperator = t.OperatorId,
                        ExitOperator = t.ExitOperatorId,
                        Status = t.Status,
                        IsPaid = t.PaymentStatus == "Paid"
                    }).ToList(),
                    TotalRevenue = transactions.Sum(t => t.TotalAmount),
                    TotalTransactions = transactions.Count
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parking history");
                return View(new HistoryViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VehicleExit(VehicleExitViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Form tidak valid" });
            }

            try
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.ParkingSpace)
                    .FirstOrDefaultAsync(v => v.VehicleNumber == model.VehicleNumber && v.IsParked);

                if (vehicle == null)
                {
                    return Json(new { success = false, message = "Kendaraan tidak ditemukan atau tidak sedang diparkir" });
                }

                var transaction = await _parkingService.ProcessCheckout(vehicle);
                transaction.PaymentMethod = model.PaymentMethod;
                transaction.PaymentAmount = model.PaymentAmount > 0 ? model.PaymentAmount : transaction.TotalAmount;
                transaction.TransactionNumber = model.TransactionNumber ?? GenerateTransactionNumber();

                await _context.SaveChangesAsync();

                // Update dashboard
                await _hubContext.Clients.All.SendAsync("UpdateDashboard");

                // Cetak tiket keluar
                var receiptData = new {
                    transactionNumber = transaction.TransactionNumber,
                    vehicleNumber = transaction.Vehicle.VehicleNumber,
                    entryTime = transaction.EntryTime,
                    exitTime = transaction.ExitTime,
                    duration = transaction.ExitTime.HasValue ? $"{(decimal)(transaction.ExitTime.Value - transaction.EntryTime).TotalHours:F1} jam" : "-",
                    amount = transaction.PaymentAmount,
                    paymentMethod = transaction.PaymentMethod
                };

                // Cetak tiket menggunakan PrintService
                var printSuccess = _printService.PrintTicket(FormatTicket(receiptData));

                return Json(new { 
                    success = true, 
                    message = "Proses keluar berhasil",
                    receiptData = receiptData,
                    printSuccess = printSuccess
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing vehicle exit");
                return Json(new { success = false, message = "Terjadi kesalahan saat mencatat keluar kendaraan" });
            }
        }

        private string FormatTicket(dynamic data)
        {
            return $@"
                TIKET PARKIR - KELUAR
                ==================
                
                No. Transaksi: {data.transactionNumber}
                No. Kendaraan: {data.vehicleNumber}
                
                Waktu Masuk: {data.entryTime:dd/MM/yyyy HH:mm}
                Waktu Keluar: {data.exitTime:dd/MM/yyyy HH:mm}
                Durasi: {Math.Floor(data.duration):F0} jam {Math.Floor((data.duration - Math.Floor(data.duration)) * 60):F0} menit
                
                Total Biaya: Rp {data.amount:N0}
                Metode Bayar: {data.paymentMethod}
                
                ==================
                Terima Kasih
            ";
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicleHistory(string vehicleNumber)
        {
            try
            {
                var transactions = await _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .Include(t => t.ParkingSpace)
                    .Where(t => t.Vehicle.VehicleNumber == vehicleNumber)
                    .OrderByDescending(t => t.EntryTime)
                    .ToListAsync();

                return PartialView("_HistoryPartial", new HistoryViewModel
                {
                    Transactions = transactions.Select(t => new TransactionHistoryItem
                    {
                        Id = t.Id.ToString(),
                        TransactionNumber = t.TransactionNumber,
                        TicketNumber = t.TicketNumber,
                        PlateNumber = t.Vehicle?.VehicleNumber ?? string.Empty,
                        VehicleType = t.Vehicle?.VehicleType ?? string.Empty,
                        EntryTime = t.EntryTime,
                        ExitTime = t.ExitTime,
                        Duration = t.ExitTime.HasValue ? $"{(decimal)(t.ExitTime.Value - t.EntryTime).TotalHours:F1} jam" : "-",
                        Amount = t.TotalAmount,
                        PaymentStatus = t.PaymentStatus,
                        PaymentMethod = t.PaymentMethod,
                        PaymentTime = t.PaymentTime,
                        EntryGate = t.EntryPoint,
                        ExitGate = t.ExitPoint,
                        EntryOperator = t.OperatorId,
                        ExitOperator = t.ExitOperatorId,
                        Status = t.Status,
                        IsPaid = t.PaymentStatus == "Paid"
                    }).ToList(),
                    TotalRevenue = transactions.Sum(t => t.TotalAmount),
                    TotalTransactions = transactions.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle history");
                return PartialView("_HistoryPartial", new HistoryViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReport(ReportViewModel model)
        {
            try
            {
                var reportFilter = new ReportFilter
                {
                    Type = model.ReportType,
                    Date = model.StartDate ?? DateTime.Today,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate
                };

                var transactions = await GetReportData(reportFilter);
                var reportData = GenerateReportData(transactions, reportFilter);

                // Generate PDF
                var pdfContent = await GeneratePdf(reportData);

                return File(pdfContent, "application/pdf", "ParkingReport.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                return Json(new { success = false, message = "Terjadi kesalahan saat mencetak laporan" });
            }
        }

        private async Task<List<ParkingTransaction>> GetReportData(ReportFilter filter)
        {
            var query = _context.ParkingTransactions
                .Include(t => t.Vehicle)
                .Include(t => t.ParkingSpace)
                .AsQueryable();

            var date = filter.Date.HasValue ? filter.Date.Value : DateTime.Today;

            switch (filter.Type)
            {
                case "Daily":
                    query = query.Where(t => t.EntryTime.Date == date.Date);
                    break;
                case "Weekly":
                    var weekStart = date.Date.AddDays(-(int)date.DayOfWeek);
                    var weekEnd = weekStart.AddDays(7);
                    query = query.Where(t => t.EntryTime.Date >= weekStart && t.EntryTime.Date < weekEnd);
                    break;
                case "Monthly":
                    query = query.Where(t => t.EntryTime.Year == date.Year && t.EntryTime.Month == date.Month);
                    break;
                case "Custom":
                    if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                    {
                        query = query.Where(t => t.EntryTime >= filter.StartDate.Value && t.EntryTime <= filter.EndDate.Value);
                    }
                    break;
            }

            return await query.ToListAsync();
        }

        private ReportData GenerateReportData(List<ParkingTransaction> transactions, ReportFilter filter)
        {
            var reportData = new ReportData
            {
                Period = GetPeriodText(filter),
                TotalTransactions = transactions.Count,
                TotalRevenue = transactions.Sum(t => t.TotalAmount),
                VehicleDistribution = transactions
                    .GroupBy(t => t.Vehicle.VehicleType)
                    .Select(g => new VehicleDistribution
                    {
                        VehicleType = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(t => t.TotalAmount)
                    })
                    .ToList()
            };

            return reportData;
        }

        private string GetPeriodText(ReportFilter filter)
        {
            var date = filter.Date.HasValue ? filter.Date.Value : DateTime.Today;

            switch (filter.Type)
            {
                case "Daily":
                    return date.ToString("dd/MM/yyyy");
                case "Weekly":
                    var weekStart = date.Date.AddDays(-(int)date.DayOfWeek);
                    var weekEnd = weekStart.AddDays(6);
                    return $"{weekStart:dd/MM/yyyy} - {weekEnd:dd/MM/yyyy}";
                case "Monthly":
                    return date.ToString("MM/yyyy");
                case "Custom":
                    return $"{filter.StartDate?.ToString("dd/MM/yyyy")} - {filter.EndDate?.ToString("dd/MM/yyyy")}";
                default:
                    return "Periode Tidak Diketahui";
            }
        }

        private async Task<byte[]> GeneratePdf(ReportData reportData)
        {
            // Implement PDF generation logic here
            // This is a placeholder for actual PDF generation
            return new byte[0];
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var activeVehicles = await _context.Vehicles.CountAsync(v => v.IsParked);
                var totalSpaces = await _context.ParkingSpaces.CountAsync();
                var availableSpaces = totalSpaces - activeVehicles;

                var today = DateTime.Today;
                var todayTransactions = await _context.ParkingTransactions
                    .Where(t => t.EntryTime.Date == today)
                    .ToListAsync();

                var todayRevenue = todayTransactions.Sum(t => t.TotalAmount);
                var todayTransactionCount = todayTransactions.Count;

                return Json(new
                {
                    success = true,
                    activeVehicles,
                    availableSpaces,
                    todayTransactions = todayTransactionCount,
                    todayRevenue
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, new { success = false, message = "Gagal mengambil statistik dashboard" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentEntriesJson()
        {
            try
            {
                var recentEntries = await _context.Vehicles
                    .Where(v => v.IsParked)
                    .OrderByDescending(v => v.EntryTime)
                    .Take(5)
                    .Select(v => new
                    {
                        timestamp = v.EntryTime.HasValue ? v.EntryTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "-",
                        vehicleNumber = v.VehicleNumber,
                        vehicleType = v.VehicleType
                    })
                    .ToListAsync();

                return Json(recentEntries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent entries");
                return StatusCode(500, new { success = false, message = "Gagal memuat data kendaraan masuk terbaru" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentExitsJson()
        {
            try
            {
                var recentExits = await _context.ParkingTransactions
                    .Where(t => t.ExitTime != default(DateTime))
                    .OrderByDescending(t => t.ExitTime)
                    .Take(5)
                    .Select(t => new
                    {
                        exitTime = t.ExitTime.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                        vehicleNumber = t.Vehicle.VehicleNumber,
                        duration = CalculateDuration(t.EntryTime, t.ExitTime),
                        totalAmount = t.TotalAmount
                    })
                    .ToListAsync();

                return Json(recentExits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent exits");
                return StatusCode(500, new { success = false, message = "Gagal memuat data kendaraan keluar terbaru" });
            }
        }

        private string CalculateDuration(DateTime start, DateTime? end)
        {
            var duration = (end ?? DateTime.Now) - start;
            var hours = Math.Floor(duration.TotalHours);
            var minutes = duration.Minutes;

            if (hours > 0)
            {
                return $"{hours:0} jam {minutes:0} menit";
            }
            return $"{minutes:0} menit";
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactionHistory(int page = 1, int pageSize = 10, string search = "", string status = "", string date = "")
        {
            try
            {
                var query = _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(t => t.Vehicle.VehicleNumber.Contains(search));
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(t => t.Status == status);
                }

                // Apply date filter
                if (!string.IsNullOrEmpty(date))
                {
                    var filterDate = DateTime.Parse(date);
                    query = query.Where(t => t.EntryTime.Date == filterDate.Date);
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var transactions = await query
                    .OrderByDescending(t => t.EntryTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        t.Id,
                        t.TransactionNumber,
                        VehicleNumber = t.Vehicle.VehicleNumber,
                        EntryTime = t.EntryTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        ExitTime = t.ExitTime.HasValue ? t.ExitTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "-",
                        t.TotalAmount,
                        t.Status
                    })
                    .ToListAsync();

                return Json(new
                {
                    transactions,
                    currentCount = transactions.Count() + ((page - 1) * pageSize),
                    totalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction history");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerateReport(string reportType, string reportFormat, string? startDate = null, string? endDate = null)
        {
            try
            {
                var query = _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .AsQueryable();

                // Apply date filter based on report type
                DateTime filterStartDate;
                DateTime filterEndDate;

                switch (reportType)
                {
                    case "daily":
                        filterStartDate = DateTime.Today;
                        filterEndDate = DateTime.Today.AddDays(1).AddTicks(-1);
                        break;
                    case "weekly":
                        filterStartDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                        filterEndDate = filterStartDate.AddDays(7).AddTicks(-1);
                        break;
                    case "monthly":
                        filterStartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        filterEndDate = filterStartDate.AddMonths(1).AddTicks(-1);
                        break;
                    case "custom":
                        if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
                        {
                            return BadRequest("Start date and end date are required for custom reports");
                        }
                        filterStartDate = DateTime.Parse(startDate);
                        filterEndDate = DateTime.Parse(endDate).AddDays(1).AddTicks(-1);
                        break;
                    default:
                        return BadRequest("Invalid report type");
                }

                query = query.Where(t => t.EntryTime >= filterStartDate && t.EntryTime <= filterEndDate);

                // Get report data
                var transactions = await query
                    .OrderByDescending(t => t.EntryTime)
                    .Select(t => new
                    {
                        t.TransactionNumber,
                        VehicleNumber = t.Vehicle.VehicleNumber,
                        VehicleType = t.Vehicle.VehicleType,
                        t.EntryTime,
                        t.ExitTime,
                        t.Amount,
                        t.TotalAmount,
                        t.Status,
                        t.PaymentMethod
                    })
                    .ToListAsync();

                // Calculate summary
                var summary = new
                {
                    TotalTransactions = transactions.Count,
                    TotalRevenue = transactions.Sum(t => t.TotalAmount),
                    CompletedTransactions = transactions.Count(t => t.Status == "Completed"),
                    ActiveTransactions = transactions.Count(t => t.Status == "Active"),
                    CancelledTransactions = transactions.Count(t => t.Status == "Cancelled"),
                    VehicleTypeDistribution = transactions
                        .GroupBy(t => t.VehicleType)
                        .Select(g => new { VehicleType = g.Key, Count = g.Count() })
                        .ToList()
                };

                // Generate report based on format
                byte[] fileContents;
                string contentType;
                string fileName;

                if (reportFormat == "pdf")
                {
                    // Generate PDF report
                    using (var ms = new MemoryStream())
                    {
                        using (var doc = new iTextSharp.text.Document())
                        {
                            var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, ms);
                            doc.Open();

                            // Add title
                            var title = new iTextSharp.text.Paragraph($"Laporan Parkir - {reportType.ToUpper()}")
                            {
                                Alignment = iTextSharp.text.Element.ALIGN_CENTER
                            };
                            doc.Add(title);
                            doc.Add(new iTextSharp.text.Paragraph($"Periode: {filterStartDate:dd/MM/yyyy} - {filterEndDate:dd/MM/yyyy}"));
                            doc.Add(new iTextSharp.text.Paragraph("\n"));

                            // Add summary
                            doc.Add(new iTextSharp.text.Paragraph("Ringkasan:"));
                            doc.Add(new iTextSharp.text.Paragraph($"Total Transaksi: {summary.TotalTransactions}"));
                            doc.Add(new iTextSharp.text.Paragraph($"Total Pendapatan: Rp {summary.TotalRevenue:N0}"));
                            doc.Add(new iTextSharp.text.Paragraph($"Transaksi Selesai: {summary.CompletedTransactions}"));
                            doc.Add(new iTextSharp.text.Paragraph($"Transaksi Aktif: {summary.ActiveTransactions}"));
                            doc.Add(new iTextSharp.text.Paragraph($"Transaksi Batal: {summary.CancelledTransactions}"));
                            doc.Add(new iTextSharp.text.Paragraph("\n"));

                            // Add transaction table
                            var table = new iTextSharp.text.pdf.PdfPTable(7)
                            {
                                WidthPercentage = 100
                            };

                            // Add headers
                            table.AddCell("No. Transaksi");
                            table.AddCell("No. Kendaraan");
                            table.AddCell("Jenis");
                            table.AddCell("Waktu Masuk");
                            table.AddCell("Waktu Keluar");
                            table.AddCell("Total");
                            table.AddCell("Status");

                            // Add data
                            foreach (var transaction in transactions)
                            {
                                table.AddCell(transaction.TransactionNumber);
                                table.AddCell(transaction.VehicleNumber);
                                table.AddCell(transaction.VehicleType);
                                table.AddCell(transaction.EntryTime.ToString("dd/MM/yyyy HH:mm:ss"));
                                table.AddCell(transaction.ExitTime == default(DateTime) ? "-" : transaction.ExitTime.Value.ToString("dd/MM/yyyy HH:mm:ss"));
                                table.AddCell($"Rp {transaction.TotalAmount:N0}");
                                table.AddCell(transaction.Status);
                            }

                            doc.Add(table);
                        }

                        fileContents = ms.ToArray();
                    }

                    contentType = "application/pdf";
                    fileName = $"parking_report_{reportType}_{DateTime.Now.ToString("yyyyMMdd")}.pdf";
                }
                else if (reportFormat == "excel")
                {
                    // Generate Excel report
                    using (var package = new OfficeOpenXml.ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Report");

                        // Add title
                        worksheet.Cells[1, 1].Value = $"Laporan Parkir - {reportType.ToUpper()}";
                        worksheet.Cells[2, 1].Value = $"Periode: {filterStartDate:dd/MM/yyyy} - {filterEndDate:dd/MM/yyyy}";

                        // Add summary
                        worksheet.Cells[4, 1].Value = "Ringkasan:";
                        worksheet.Cells[5, 1].Value = "Total Transaksi:";
                        worksheet.Cells[5, 2].Value = summary.TotalTransactions;
                        worksheet.Cells[6, 1].Value = "Total Pendapatan:";
                        worksheet.Cells[6, 2].Value = $"Rp {summary.TotalRevenue:N0}";
                        worksheet.Cells[7, 1].Value = "Transaksi Selesai:";
                        worksheet.Cells[7, 2].Value = summary.CompletedTransactions;
                        worksheet.Cells[8, 1].Value = "Transaksi Aktif:";
                        worksheet.Cells[8, 2].Value = summary.ActiveTransactions;
                        worksheet.Cells[9, 1].Value = "Transaksi Batal:";
                        worksheet.Cells[9, 2].Value = summary.CancelledTransactions;

                        // Add headers
                        var headers = new[] { "No. Transaksi", "No. Kendaraan", "Jenis", "Waktu Masuk", "Waktu Keluar", "Total", "Status" };
                        for (var i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cells[11, i + 1].Value = headers[i];
                        }

                        // Add data
                        var row = 12;
                        foreach (var transaction in transactions)
                        {
                            worksheet.Cells[row, 1].Value = transaction.TransactionNumber;
                            worksheet.Cells[row, 2].Value = transaction.VehicleNumber;
                            worksheet.Cells[row, 3].Value = transaction.VehicleType;
                            worksheet.Cells[row, 4].Value = transaction.EntryTime.ToString("dd/MM/yyyy HH:mm:ss");
                            worksheet.Cells[row, 5].Value = transaction.ExitTime == default(DateTime) ? "-" : transaction.ExitTime.Value.ToString("dd/MM/yyyy HH:mm:ss");
                            worksheet.Cells[row, 6].Value = $"Rp {transaction.TotalAmount:N0}";
                            worksheet.Cells[row, 7].Value = transaction.Status;
                            row++;
                        }

                        // Auto fit columns
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        fileContents = package.GetAsByteArray();
                    }

                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileName = $"parking_report_{reportType}_{DateTime.Now:yyyyMMdd}.xlsx";
                }
                else
                {
                    return BadRequest("Invalid report format");
                }

                return File(fileContents, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [Authorize]
        public IActionResult QuickAccess()
        {
            // Get some basic statistics for the dashboard
            var totalSpaces = _context.ParkingSpaces.Count();
            var availableSpaces = _context.ParkingSpaces.Count(x => !x.IsOccupied);
            var occupiedSpaces = _context.ParkingSpaces.Count(x => x.IsOccupied);
            
            // Get today's transactions
            var today = DateTime.Today;
            var todayTransactions = _context.ParkingTransactions
                .Where(x => x.EntryTime.Date == today)
                .Select(x => x.TotalAmount)
                .ToList();
            var todayRevenue = todayTransactions.Sum();
            
            ViewBag.TotalSpaces = totalSpaces;
            ViewBag.AvailableSpaces = availableSpaces;
            ViewBag.OccupiedSpaces = occupiedSpaces;
            ViewBag.TodayRevenue = todayRevenue;
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessExitTicket(string barcode)
        {
            try
            {
                // Cari data kendaraan berdasarkan barcode
                var parkingTransaction = await _context.ParkingTransactions
                    .Include(p => p.Vehicle)
                    .Include(p => p.ParkingSpace)
                    .FirstOrDefaultAsync(p => p.TransactionNumber == barcode && p.Status == "Active");

                if (parkingTransaction == null)
                    return Json(new { success = false, message = "Tiket tidak valid" });

                // Hitung durasi dan biaya
                var duration = DateTime.Now - parkingTransaction.EntryTime;
                var totalAmount = CalculateParkingFee(duration, parkingTransaction.HourlyRate);

                return Json(new {
                    success = true,
                    vehicleNumber = parkingTransaction.Vehicle.VehicleNumber,
                    entryTime = parkingTransaction.EntryTime,
                    entryPhoto = parkingTransaction.Vehicle.EntryPhotoPath,
                    duration = (decimal)duration.TotalHours,
                    amount = totalAmount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteExit(string transactionNumber, decimal paymentAmount, string paymentMethod)
        {
            try
            {
                var transaction = await _context.ParkingTransactions
                    .Include(p => p.Vehicle)
                    .Include(p => p.ParkingSpace)
                    .FirstOrDefaultAsync(p => p.TransactionNumber == transactionNumber);

                if (transaction == null)
                    return Json(new { success = false, message = "Transaksi tidak ditemukan" });

                // Update status transaksi
                transaction.ExitTime = DateTime.Now;
                transaction.PaymentAmount = paymentAmount;
                transaction.PaymentMethod = paymentMethod;
                transaction.PaymentStatus = "Paid";
                transaction.Status = "Completed";

                // Update status kendaraan dan slot parkir
                transaction.Vehicle.IsParked = false;
                transaction.Vehicle.ExitTime = DateTime.Now;
                transaction.ParkingSpace.IsOccupied = false;
                transaction.ParkingSpace.CurrentVehicleId = null;

                await _context.SaveChangesAsync();

                // Kirim sinyal untuk membuka gate
                await _hubContext.Clients.All.SendAsync("OpenExitGate", transaction.ParkingSpace.SpaceNumber);

                // Cetak tiket keluar
                var receiptData = new {
                    transactionNumber = transaction.TransactionNumber,
                    vehicleNumber = transaction.Vehicle.VehicleNumber,
                    entryTime = transaction.EntryTime,
                    exitTime = transaction.ExitTime,
                    duration = transaction.ExitTime.HasValue ? $"{(decimal)(transaction.ExitTime.Value - transaction.EntryTime).TotalHours:F1} jam" : "-",
                    amount = transaction.PaymentAmount,
                    paymentMethod = transaction.PaymentMethod
                };

                // Cetak tiket menggunakan PrintService
                var printSuccess = _printService.PrintTicket(FormatTicket(receiptData));

                return Json(new { 
                    success = true, 
                    message = "Proses keluar berhasil",
                    receiptData = receiptData,
                    printSuccess = printSuccess
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class TransactionRequest
    {
        public string VehicleNumber { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Cash";
    }
}