using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Web.Data;
using ParkIRC.Web.Models;
using ParkIRC.Web.Services;
using System.Threading.Tasks;

namespace ParkIRC.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemService _systemService;

        public SystemController(
            ApplicationDbContext context,
            ISystemService systemService)
        {
            _context = context;
            _systemService = systemService;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var systemStatus = new SystemStatus
                {
                    DatabaseConnection = await _systemService.CheckDatabaseConnection(),
                    PrinterConnection = await _systemService.CheckPrinterConnection(),
                    CameraConnection = await _systemService.CheckCameraConnection(),
                    StorageStatus = await _systemService.CheckStorageStatus()
                };

                return Ok(new
                {
                    timestamp = DateTime.Now,
                    isConnected = systemStatus.DatabaseConnection && systemStatus.PrinterConnection && systemStatus.CameraConnection,
                    database = systemStatus.DatabaseConnection,
                    printer = systemStatus.PrinterConnection,
                    camera = systemStatus.CameraConnection,
                    storage = new
                    {
                        totalSpace = systemStatus.StorageStatus.TotalSpace,
                        freeSpace = systemStatus.StorageStatus.FreeSpace,
                        usedSpace = systemStatus.StorageStatus.UsedSpace,
                        usagePercentage = systemStatus.StorageStatus.UsagePercentage
                    }
                });
            }
            catch (Exception ex)
            {
                // Assuming _logger is defined elsewhere in the class
                // _logger.LogError(ex, "Error checking system status");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        public class SystemStatus
        {
            public bool DatabaseConnection { get; set; }
            public bool PrinterConnection { get; set; }
            public bool CameraConnection { get; set; }
            public StorageStatus StorageStatus { get; set; }
        }

        public class StorageStatus
        {
            public long TotalSpace { get; set; }
            public long FreeSpace { get; set; }
            public long UsedSpace { get; set; }
            public double UsagePercentage { get; set; }
        }
    }
}