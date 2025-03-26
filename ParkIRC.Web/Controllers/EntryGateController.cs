using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Web.Data;
using ParkIRC.Data.Models;
using ParkIRC.Data.Services;
using ParkIRC.Data.Hub;
using System.Threading.Tasks;

namespace ParkIRC.Web.Controllers
{
    [Authorize(Roles = "Admin,Operator")]
    public class EntryGateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EntryGateController> _logger;
        private readonly ConnectionStatusService _connectionStatusService;
        private readonly ICameraService _cameraService;
        private readonly StorageService _storageService;
        private readonly ITicketService _ticketService;
        private readonly IPrinterService _printerService;
        private readonly IHubContext<ParkingHub> _parkingHubContext;

        public EntryGateController(
            ApplicationDbContext context,
            ILogger<EntryGateController> logger,
            ConnectionStatusService connectionStatusService,
            ICameraService cameraService,
            StorageService storageService,
            ITicketService ticketService,
            IPrinterService printerService,
            IHubContext<ParkingHub> parkingHubContext)
        {
            _context = context;
            _logger = logger;
            _connectionStatusService = connectionStatusService;
            _cameraService = cameraService;
            _storageService = storageService;
            _ticketService = ticketService;
            _printerService = printerService;
            _parkingHubContext = parkingHubContext;
        }

        public async Task<IActionResult> Index()
        {
            var model = new EntryGateMonitoringViewModel();
            
            // Get entry gates statuses
            model.EntryGates = await _context.EntryGates
                .Where(g => g.IsActive)
                .Select(g => new EntryGateStatusViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsOnline = g.IsOnline,
                    IsOpen = g.IsOpen,
                    LastActivity = g.LastActivity,
                    VehiclesProcessed = _context.ParkingTransactions
                        .Count(t => t.EntryPoint == g.Id && t.EntryTime.Date == DateTime.Today)
                })
                .ToListAsync();
            
            // Get recent transactions
            model.RecentTransactions = await _context.ParkingTransactions
                .Include(t => t.Vehicle)
                .Where(t => t.EntryTime > DateTime.Now.AddHours(-1))
                .OrderByDescending(t => t.EntryTime)
                .Take(10)
                .Select(t => new RecentTransactionViewModel
                {
                    Id = t.Id.ToString(),
                    TicketNumber = t.TicketNumber,
                    VehicleNumber = t.Vehicle.VehicleNumber,
                    VehicleType = t.Vehicle.VehicleType,
                    EntryTime = t.EntryTime,
                    EntryPoint = t.EntryPoint
                })
                .ToListAsync();
            
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddEntryGate(EntryGate gate)
        {
            if (ModelState.IsValid)
            {
                gate.Id = $"ENTRY{await _context.EntryGates.CountAsync() + 1}";
                gate.IsActive = true;
                gate.LastActivity = DateTime.Now;
                
                _context.EntryGates.Add(gate);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"New entry gate added: {gate.Name}");
                
                return RedirectToAction(nameof(Index));
            }
            
            return View(gate);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEntryGate(string id, bool isActive)
        {
            var gate = await _context.EntryGates.FindAsync(id);
            if (gate == null)
            {
                return NotFound();
            }
            
            gate.IsActive = isActive;
            gate.LastActivity = DateTime.Now;
            
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Entry gate {id} status updated: Active = {isActive}");
            
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetGateStatus(string id)
        {
            var gate = await _context.EntryGates.FindAsync(id);
            if (gate == null)
            {
                return NotFound();
            }
            
            return Json(new
            {
                id = gate.Id,
                name = gate.Name,
                isOnline = gate.IsOnline,
                isOpen = gate.IsOpen,
                lastActivity = gate.LastActivity
            });
        }

        [HttpPost]
        public async Task<IActionResult> CaptureEntry([FromBody] EntryRequest request)
        {
            try {
                // Capture vehicle photo
                var imageBytes = await _cameraService.TakePhoto();
                
                // Generate unique filename
                string fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{request.GateId}.jpg";
                
                // Save photo
                string imagePath = await _storageService.SaveImage(fileName, imageBytes);
                
                // Create parking transaction
                var ticket = await _ticketService.GenerateTicketAsync(new Vehicle
                {
                    VehicleNumber = request.VehicleNumber,
                    VehicleType = request.VehicleType,
                    EntryTime = DateTime.Now
                }, User.GetOperatorId());

                return Ok(new { 
                    ticketNumber = ticket.TicketNumber,
                    imagePath = imagePath
                });
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error processing entry");
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignSpace(VehicleEntryRequest request)
        {
            try
            {
                // Automatic space assignment
                var availableSpace = await _context.ParkingSpaces
                    .Where(s => !s.IsOccupied && s.SpaceType == request.VehicleType)
                    .OrderBy(s => s.SpaceNumber)
                    .FirstOrDefaultAsync();

                if (availableSpace == null)
                {
                    return BadRequest(new { message = "No parking space available" });
                }

                // Create vehicle
                var vehicle = new Vehicle
                {
                    VehicleNumber = request.VehicleNumber,
                    VehicleType = request.VehicleType,
                    EntryTime = DateTime.Now
                };

                // Generate ticket
                var ticket = await _ticketService.GenerateTicketAsync(vehicle, User.GetOperatorId());

                // Create transaction
                var transaction = new ParkingTransaction
                {
                    TicketNumber = ticket.TicketNumber,
                    Vehicle = vehicle,
                    EntryTime = DateTime.Now,
                    ParkingSpaceId = availableSpace.Id,
                    EntryPoint = request.GateId,
                    ImagePath = request.ImagePath,
                    OperatorId = User.GetOperatorId()
                };

                _context.ParkingTransactions.Add(transaction);
                
                // Update space status
                availableSpace.IsOccupied = true;
                availableSpace.CurrentVehicleId = vehicle.Id;

                await _context.SaveChangesAsync();

                // Print ticket
                await _printerService.PrintTicketAsync(ticket.TicketNumber, request.VehicleNumber, DateTime.Now, request.VehicleType);

                return Ok(new { 
                    ticketNumber = ticket.TicketNumber,
                    spaceNumber = availableSpace.SpaceNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in automatic space assignment");
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessVehicleEntry(VehicleEntryRequest request)
        {
            try
            {
                // Cari kendaraan berdasarkan nomor plat atau buat baru jika belum ada
                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.PlateNumber == request.PlateNumber);

                if (vehicle == null)
                {
                    vehicle = new Vehicle
                    {
                        PlateNumber = request.PlateNumber,
                        Type = request.VehicleType,
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    };
                    _context.Vehicles.Add(vehicle);
                    await _context.SaveChangesAsync();
                }

                // Cari space parkir yang tersedia
                var parkingSpace = await _context.ParkingSpaces
                    .FirstOrDefaultAsync(ps => ps.IsActive && !ps.IsOccupied && ps.Type == request.VehicleType);

                if (parkingSpace == null)
                {
                    TempData["ErrorMessage"] = "Tidak ada ruang parkir tersedia untuk jenis kendaraan ini";
                    return RedirectToAction(nameof(Operator), new { gateId = request.GateId });
                }

                // Buat entry vehicle baru
                var vehicleEntry = new VehicleEntry
                {
                    PlateNumber = request.PlateNumber,
                    VehicleType = request.VehicleType,
                    EntryTime = DateTime.Now,
                    EntryPhotoPath = request.PhotoPath,
                    VehicleId = vehicle.Id,
                    ParkingSpaceId = parkingSpace.Id,
                    OperatorId = User.Identity.Name,
                    Notes = request.Notes
                };

                _context.VehicleEntries.Add(vehicleEntry);

                // Buat transaksi parkir baru
                var transaction = new ParkingTransaction
                {
                    VehicleId = vehicle.Id,
                    ParkingSpaceId = parkingSpace.Id,
                    EntryTime = DateTime.Now,
                    OperatorId = User.Identity.Name,
                    EntryPhotoPath = request.PhotoPath,
                    CreatedAt = DateTime.Now
                };

                _context.ParkingTransactions.Add(transaction);

                // Update status ruang parkir menjadi terisi
                parkingSpace.IsOccupied = true;
                _context.ParkingSpaces.Update(parkingSpace);

                await _context.SaveChangesAsync();

                // Kirim notifikasi melalui SignalR
                await _parkingHubContext.Clients.All.SendAsync("ReceiveVehicleEntry", transaction);
                await _parkingHubContext.Clients.All.SendAsync("ReceiveSpaceUpdate", parkingSpace);
                
                // Cetak tiket jika diminta
                if (request.PrintTicket)
                {
                    await _parkingHubContext.Clients.All.SendAsync("PrintTicket", new
                    {
                        plateNumber = request.PlateNumber,
                        vehicleType = request.VehicleType,
                        entryTime = transaction.EntryTime,
                        transactionId = transaction.Id
                    });
                }
                
                // Buka palang
                await _parkingHubContext.Clients.All.SendAsync("OpenEntryGate", request.GateId);

                TempData["SuccessMessage"] = "Kendaraan berhasil masuk area parkir";
                return RedirectToAction(nameof(Operator), new { gateId = request.GateId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Terjadi kesalahan: {ex.Message}";
                return RedirectToAction(nameof(Operator), new { gateId = request.GateId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessButtonPress(string gateId)
        {
            try
            {
                // Kirim notifikasi ke semua klien bahwa tombol masuk ditekan
                await _parkingHubContext.Clients.All.SendAsync("EntryButtonPressed", gateId);
                
                // Kirim perintah ke kamera untuk mengambil gambar
                await _parkingHubContext.Clients.All.SendAsync("TriggerCamera", gateId);
                
                return Ok(new { success = true, message = "Push button processed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PrintTicket(int transactionId)
        {
            try
            {
                var transaction = await _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .FirstOrDefaultAsync(t => t.Id == transactionId);

                if (transaction == null)
                {
                    return NotFound(new { success = false, message = "Transaksi tidak ditemukan" });
                }

                // Kirim perintah untuk mencetak tiket
                await _parkingHubContext.Clients.All.SendAsync("PrintTicket", new
                {
                    plateNumber = transaction.Vehicle?.PlateNumber,
                    vehicleType = transaction.Vehicle?.Type,
                    entryTime = transaction.EntryTime,
                    transactionId = transaction.Id
                });

                return Ok(new { success = true, message = "Print command sent" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Operator(string gateId = "GATE-01")
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve recent vehicle entries
            var recentEntries = await _context.VehicleEntries
                .OrderByDescending(ve => ve.EntryTime)
                .Take(10)
                .ToListAsync();

            var viewModel = new EntryGateViewModel
            {
                GateId = gateId,
                OperatorName = $"{currentUser.FirstName} {currentUser.LastName}",
                Status = "Ready",
                IsCameraActive = false,
                IsPrinterActive = true,
                IsOfflineMode = false,
                LastSync = DateTime.Now,
                RecentEntries = recentEntries
            };

            return View(viewModel);
        }
    }
}