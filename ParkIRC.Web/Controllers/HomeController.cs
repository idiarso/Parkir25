using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Web.Data;
using ParkIRC.Web.Models;
using ParkIRC.Web.Services;
using System.Threading.Tasks;

namespace ParkIRC.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IParkingService _parkingService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IOfflineDataService _offlineDataService;

        public HomeController(
            ApplicationDbContext context,
            IParkingService parkingService,
            IMaintenanceService maintenanceService,
            IOfflineDataService offlineDataService)
        {
            _context = context;
            _parkingService = parkingService;
            _maintenanceService = maintenanceService;
            _offlineDataService = offlineDataService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalVehicles = await _context.ParkingTransactions.CountAsync(),
                ActiveVehicles = await _context.ParkingTransactions
                    .CountAsync(pt => pt.ExitTime == null),
                RevenueToday = await _context.ParkingTransactions
                    .Where(pt => pt.EntryTime >= DateTime.Today)
                    .SumAsync(pt => pt.Amount),
                MaintenanceIssues = await _maintenanceService.GetActiveIssues(),
                OfflineDataCount = await _offlineDataService.GetPendingCount()
            };

            return View(viewModel);
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
