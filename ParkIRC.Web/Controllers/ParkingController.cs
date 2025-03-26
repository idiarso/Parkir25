using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Web.Data;
using ParkIRC.Web.Models;
using ParkIRC.Web.Services;
using ParkIRC.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using ParkIRC.Web.Extensions;
using ParkIRC.Web.Hubs;

namespace ParkIRC.Web.Controllers
{
    public class ParkingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IParkingService _parkingService;

        public ParkingController(
            ApplicationDbContext context,
            IParkingService parkingService)
        {
            _context = context;
            _parkingService = parkingService;
        }

        public async Task<IActionResult> Index()
        {
            var parkingTransactions = await _context.ParkingTransactions
                .OrderByDescending(t => t.EntryTime)
                .Take(50)
                .ToListAsync();

            return View(parkingTransactions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _context.ParkingTransactions
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }
    }
}
