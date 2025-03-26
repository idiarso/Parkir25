using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Web.Data;
using ParkIRC.Web.Models;
using ParkIRC.Web.Services;
using System.Threading.Tasks;
using System.Linq;

namespace ParkIRC.Web.Controllers
{
    public class GateOperationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICameraService _cameraService;
        private readonly IPrinterService _printerService;

        public GateOperationsController(
            ApplicationDbContext context,
            ICameraService cameraService,
            IPrinterService printerService)
        {
            _context = context;
            _cameraService = cameraService;
            _printerService = printerService;
        }

        public async Task<IActionResult> Index()
        {
            var gates = await _context.EntryGates
                .Include(g => g.Camera)
                .Include(g => g.Printer)
                .ToListAsync();

            return View(gates);
        }

        public async Task<IActionResult> Details(int id)
        {
            var gate = await _context.EntryGates
                .Include(g => g.Camera)
                .Include(g => g.Printer)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gate == null)
            {
                return NotFound();
            }

            return View(gate);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Location,IsActive,CameraId,PrinterId")] EntryGate entryGate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entryGate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(entryGate);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entryGate = await _context.EntryGates.FindAsync(id);
            if (entryGate == null)
            {
                return NotFound();
            }
            return View(entryGate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Location,IsActive,CameraId,PrinterId")] EntryGate entryGate)
        {
            if (id != entryGate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entryGate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntryGateExists(entryGate.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(entryGate);
        }

        private bool EntryGateExists(int id)
        {
            return _context.EntryGates.Any(e => e.Id == id);
        }
    }
}
