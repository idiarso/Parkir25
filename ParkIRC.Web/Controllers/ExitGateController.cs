using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Web.Data;
using ParkIRC.Data.Models;
using ParkIRC.Data.Services;
using ParkIRC.Data.Hub;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace ParkIRC.Web.Controllers
{
    [Authorize]
    public class ExitGateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ParkingHub> _hubContext;
        private readonly PrintService _printService;

        public ExitGateController(
            ApplicationDbContext context,
            IHubContext<ParkingHub> hubContext,
            PrintService printService)
        {
            _context = context;
            _hubContext = hubContext;
            _printService = printService;
        }

        // GET: /ExitGate
        public IActionResult Index()
        {
            return View();
        }

        // GET: /ExitGate/Operator
        public async Task<IActionResult> Operator(string gateId = "EXIT-01")
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve recent vehicle exits
            var recentExits = await _context.VehicleExits
                .OrderByDescending(ve => ve.ExitTime)
                .Take(10)
                .ToListAsync();

            var viewModel = new ExitGateViewModel
            {
                GateId = gateId,
                OperatorName = $"{currentUser.FirstName} {currentUser.LastName}",
                Status = "Ready",
                IsCameraActive = true,
                IsPrinterActive = true,
                IsOfflineMode = false,
                LastSync = DateTime.Now,
                RecentExits = recentExits
            };

            return View(viewModel);
        }

        // POST: /ExitGate/ProcessVehicleExit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessVehicleExit(VehicleExitRequest request)
        {
            try
            {
                // Cari transaksi parkir yang aktif berdasarkan plat nomor
                var transaction = await _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .Include(t => t.ParkingSpace)
                    .Where(t => t.Vehicle.PlateNumber == request.PlateNumber && t.ExitTime == null)
                    .FirstOrDefaultAsync();

                if (transaction == null)
                {
                    TempData["ErrorMessage"] = "Tidak ada transaksi parkir aktif untuk kendaraan ini";
                    return RedirectToAction(nameof(Operator), new { gateId = request.GateId });
                }

                // Hitung durasi parkir dan biaya
                var entryTime = transaction.EntryTime;
                var exitTime = DateTime.Now;
                var duration = exitTime - entryTime;

                // Hitung biaya berdasarkan tipe kendaraan dan durasi
                decimal rate = transaction.Vehicle.Type.ToLower() == "mobil" ? 5000 : 2000; // Tarif per jam
                decimal cost = (decimal)Math.Ceiling(duration.TotalHours) * rate;

                // Update transaksi
                transaction.ExitTime = exitTime;
                transaction.ExitPhotoPath = request.PhotoPath;
                transaction.Duration = (decimal)duration.TotalHours;
                transaction.Cost = cost;
                transaction.UpdatedAt = DateTime.Now;

                // Bebas ruang parkir
                if (transaction.ParkingSpace != null)
                {
                    transaction.ParkingSpace.IsOccupied = false;
                    _context.ParkingSpaces.Update(transaction.ParkingSpace);
                }

                _context.ParkingTransactions.Update(transaction);

                // Buat record exit kendaraan
                var vehicleExit = new VehicleExit
                {
                    PlateNumber = request.PlateNumber,
                    VehicleType = transaction.Vehicle.Type,
                    ExitTime = exitTime,
                    ExitPhotoPath = request.PhotoPath,
                    VehicleId = transaction.VehicleId,
                    ParkingSpaceId = transaction.ParkingSpaceId,
                    OperatorId = User.Identity.Name,
                    TransactionId = transaction.Id,
                    Cost = cost,
                    Duration = (decimal)duration.TotalHours,
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = "Completed",
                    Notes = request.Notes
                };

                _context.VehicleExits.Add(vehicleExit);
                await _context.SaveChangesAsync();

                // Kirim notifikasi melalui SignalR
                await _hubContext.Clients.All.SendAsync("ReceiveVehicleExit", transaction);
                
                if (transaction.ParkingSpace != null)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveSpaceUpdate", transaction.ParkingSpace);
                }
                
                // Print receipt jika diminta
                if (request.PrintReceipt)
                {
                    await _hubContext.Clients.All.SendAsync("PrintReceipt", new
                    {
                        plateNumber = request.PlateNumber,
                        vehicleType = transaction.Vehicle.Type,
                        entryTime = transaction.EntryTime,
                        exitTime = exitTime,
                        duration = duration,
                        cost = cost,
                        transactionId = transaction.Id,
                        paymentMethod = request.PaymentMethod
                    });
                }
                
                // Buka palang keluar
                await _hubContext.Clients.All.SendAsync("OpenExitGate", request.GateId);

                TempData["SuccessMessage"] = "Kendaraan berhasil keluar area parkir";
                return RedirectToAction(nameof(Operator), new { gateId = request.GateId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Terjadi kesalahan: {ex.Message}";
                return RedirectToAction(nameof(Operator), new { gateId = request.GateId });
            }
        }

        // POST: /ExitGate/ProcessButtonPress
        [HttpPost]
        public async Task<IActionResult> ProcessButtonPress(string gateId)
        {
            try
            {
                // Kirim notifikasi ke semua klien bahwa tombol keluar ditekan
                await _hubContext.Clients.All.SendAsync("ExitButtonPressed", gateId);
                
                // Kirim perintah ke kamera untuk mengambil gambar
                await _hubContext.Clients.All.SendAsync("TriggerCamera", gateId);
                
                return Ok(new { success = true, message = "Push button processed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: /ExitGate/GetGateStatus
        [HttpGet]
        public async Task<IActionResult> GetGateStatus(string gateId)
        {
            try
            {
                // Cek status perangkat gate
                bool isCameraActive = true; // Di implementasi sebenarnya, cek hardware
                bool isPrinterActive = true; // Di implementasi sebenarnya, cek hardware
                bool isOnline = true; // Di implementasi sebenarnya, cek koneksi

                // Mengembalikan status gate
                return Json(new
                {
                    isOnline,
                    isCameraActive,
                    isPrinterActive,
                    lastSync = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: /ExitGate/FindVehicle
        [HttpGet]
        public async Task<IActionResult> FindVehicle(string plateNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(plateNumber))
                {
                    return BadRequest(new { success = false, message = "Nomor plat tidak valid" });
                }

                var transaction = await _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .Include(t => t.ParkingSpace)
                    .Where(t => t.Vehicle.PlateNumber == plateNumber && t.ExitTime == null)
                    .FirstOrDefaultAsync();

                if (transaction == null)
                {
                    return NotFound(new { success = false, message = "Kendaraan tidak ditemukan atau sudah keluar" });
                }

                // Hitung durasi dan biaya sementara
                var duration = DateTime.Now - transaction.EntryTime;
                decimal rate = transaction.Vehicle.Type.ToLower() == "mobil" ? 5000 : 2000; // Tarif per jam
                decimal cost = (decimal)Math.Ceiling(duration.TotalHours) * rate;

                return Ok(new
                {
                    success = true,
                    plateNumber = transaction.Vehicle.PlateNumber,
                    vehicleType = transaction.Vehicle.Type,
                    entryTime = transaction.EntryTime,
                    durationHours = Math.Ceiling(duration.TotalHours),
                    durationMinutes = Math.Ceiling(duration.TotalMinutes),
                    parkingSpaceId = transaction.ParkingSpace?.Name,
                    cost = cost
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: /ExitGate/PrintReceipt
        [HttpPost]
        public async Task<IActionResult> PrintReceipt(int transactionId)
        {
            try
            {
                var exit = await _context.VehicleExits
                    .Include(e => e.Transaction)
                    .Include(e => e.Vehicle)
                    .FirstOrDefaultAsync(e => e.TransactionId == transactionId);

                if (exit == null)
                {
                    return NotFound(new { success = false, message = "Transaksi tidak ditemukan" });
                }

                // Kirim perintah untuk mencetak receipt
                await _hubContext.Clients.All.SendAsync("PrintReceipt", new
                {
                    plateNumber = exit.Vehicle?.PlateNumber,
                    vehicleType = exit.Vehicle?.Type,
                    entryTime = exit.Transaction?.EntryTime,
                    exitTime = exit.ExitTime,
                    duration = exit.Duration,
                    cost = exit.Cost,
                    transactionId = exit.TransactionId,
                    paymentMethod = exit.PaymentMethod
                });

                return Ok(new { success = true, message = "Print command sent" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class VehicleExitRequest
    {
        public string PlateNumber { get; set; } = string.Empty;
        public string GateId { get; set; } = string.Empty;
        public string? PhotoPath { get; set; }
        public bool PrintReceipt { get; set; } = true;
        public string PaymentMethod { get; set; } = "Cash";
        public string? Notes { get; set; }
    }

    public class ExitGateViewModel
    {
        public string GateId { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsCameraActive { get; set; }
        public bool IsPrinterActive { get; set; }
        public bool IsOfflineMode { get; set; }
        public DateTime LastSync { get; set; }
        public List<VehicleExit> RecentExits { get; set; } = new();
    }
}