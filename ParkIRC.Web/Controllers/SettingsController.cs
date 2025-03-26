using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System;
using ParkIRC.Web.Data;
using ParkIRC.Web.Models;

namespace ParkIRC.Web.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new SiteSettings
                {
                    SiteName = "ParkIRC",
                    LogoUrl = null,
                    ShowLogo = true,
                    Theme = "light",
                    CurrencySymbol = "$",
                    TimeFormat = "HH:mm",
                    DateFormat = "yyyy-MM-dd"
                };
                await _context.SiteSettings.AddAsync(settings);
                await _context.SaveChangesAsync();
            }

            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SiteSettings settings)
        {
            if (ModelState.IsValid)
            {
                var existingSettings = await _context.SiteSettings.FirstOrDefaultAsync();
                if (existingSettings != null)
                {
                    existingSettings.SiteName = settings.SiteName;
                    existingSettings.LogoUrl = settings.LogoUrl;
                    existingSettings.ShowLogo = settings.ShowLogo;
                    existingSettings.Theme = settings.Theme;
                    existingSettings.CurrencySymbol = settings.CurrencySymbol;
                    existingSettings.TimeFormat = settings.TimeFormat;
                    existingSettings.DateFormat = settings.DateFormat;
                    
                    _context.Update(existingSettings);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    await _context.SiteSettings.AddAsync(settings);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(settings);
        }
    }
}
