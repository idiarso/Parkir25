using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Data.Models;
using ParkIRC.Data.Services;
using System.Threading.Tasks;

namespace ParkIRC.Web.Controllers
{
    [Authorize]
    public class RatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRateService _rateService;

        public RatesController(
            ApplicationDbContext context,
            IRateService rateService)
        {
            _context = context;
            _rateService = rateService;
        }

        // Display list of parking rates
        public async Task<IActionResult> Index()
        {
            var rates = await _context.Rates
                .OrderBy(r => r.VehicleType)
                .ToListAsync();

            return View(rates);
        }

        // Create new rate
        public IActionResult Create()
        {
            var rate = new Rate
            {
                VehicleType = "car", // Default value, will be changed by user
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            return View(rate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rate rate)
        {
            if (ModelState.IsValid)
            {
                rate.CreatedAt = DateTime.Now;
                rate.UpdatedAt = DateTime.Now;
                
                await _context.Rates.AddAsync(rate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rate);
        }

        // Edit rate
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rate = await _context.Rates.FindAsync(id);
            if (rate == null)
            {
                return NotFound();
            }
            return View(rate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Rate rate)
        {
            if (id != rate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    rate.UpdatedAt = DateTime.Now;
                    
                    _context.Update(rate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await RateExists(rate.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(rate);
        }

        // Delete rate
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rate = await _context.Rates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rate == null)
            {
                return NotFound();
            }

            return View(rate);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rate = await _context.Rates.FindAsync(id);
            if (rate != null)
            {
                _context.Rates.Remove(rate);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> RateExists(int id)
        {
            return await _context.Rates.AnyAsync(e => e.Id == id);
        }
    }
}