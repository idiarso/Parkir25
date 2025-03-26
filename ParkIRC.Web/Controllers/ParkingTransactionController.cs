using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Web.Data;
using ParkIRC.Web.Models;
using ParkIRC.Web.Services;
using System.Threading.Tasks;
using ParkIRC.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ParkIRC.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkingTransactionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ParkingHub> _parkingHubContext;
        private readonly IParkingService _parkingService;
        private readonly ILogger<ParkingTransactionController> _logger;

        public ParkingTransactionController(
            ApplicationDbContext context,
            IHubContext<ParkingHub> parkingHubContext,
            IParkingService parkingService,
            ILogger<ParkingTransactionController> logger)
        {
            _context = context;
            _parkingHubContext = parkingHubContext;
            _parkingService = parkingService;
            _logger = logger;
        }

        // POST: api/ParkingTransaction/entry
        [HttpPost("entry")]
        public async Task<IActionResult> VehicleEntry([FromBody] VehicleEntryRequest request)
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
                    return BadRequest("Tidak ada ruang parkir tersedia untuk jenis kendaraan ini");
                }

                // Buat transaksi parkir baru
                var transaction = new ParkingTransaction
                {
                    VehicleId = vehicle.Id,
                    ParkingSpaceId = parkingSpace.Id,
                    EntryTime = DateTime.Now,
                    OperatorId = request.OperatorId,
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

                return Ok(new
                {
                    Success = true,
                    Message = "Kendaraan berhasil masuk",
                    Data = transaction
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during vehicle entry");
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        // POST: api/ParkingTransaction/exit
        [HttpPost("exit")]
        public async Task<IActionResult> VehicleExit([FromBody] VehicleExitRequest request)
        {
            try
            {
                // Cari transaksi parkir yang aktif (belum keluar)
                var transaction = await _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .Include(t => t.ParkingSpace)
                    .FirstOrDefaultAsync(t => t.Id == request.TransactionId && t.ExitTime == null);

                if (transaction == null)
                {
                    return BadRequest("Transaksi parkir tidak ditemukan atau kendaraan sudah keluar");
                }

                // Update data transaksi
                transaction.ExitTime = DateTime.Now;
                transaction.ExitPhotoPath = request.PhotoPath;
                transaction.Fee = request.Fee;
                transaction.IsPaid = request.IsPaid;
                transaction.UpdatedAt = DateTime.Now;

                // Update status ruang parkir menjadi tersedia kembali
                if (transaction.ParkingSpace != null)
                {
                    transaction.ParkingSpace.IsOccupied = false;
                    _context.ParkingSpaces.Update(transaction.ParkingSpace);
                }

                _context.ParkingTransactions.Update(transaction);
                await _context.SaveChangesAsync();

                // Kirim notifikasi melalui SignalR
                await _parkingHubContext.Clients.All.SendAsync("ReceiveVehicleExit", transaction);
                if (transaction.ParkingSpace != null)
                {
                    await _parkingHubContext.Clients.All.SendAsync("ReceiveSpaceUpdate", transaction.ParkingSpace);
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Kendaraan berhasil keluar",
                    Data = transaction
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during vehicle exit");
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        public class VehicleExitRequest
        {
            public int TransactionId { get; set; }
            public decimal Fee { get; set; }
            public bool IsPaid { get; set; }
            public string? PhotoPath { get; set; }
        }
    }
}