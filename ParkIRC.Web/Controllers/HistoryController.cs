using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Data;
using ParkIRC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using ParkIRC.Web.ViewModels;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Geom;
using OfficeOpenXml;

namespace ParkIRC.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HistoryController> _logger;

        public HistoryController(ApplicationDbContext context, ILogger<HistoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                startDate = startDate ?? DateTime.Today;
                endDate = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var transactions = await _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .Include(t => t.ParkingSpace)
                    .Where(t => t.EntryTime.Date >= startDate.Value.Date && t.EntryTime.Date <= endDate.Value.Date)
                    .OrderByDescending(t => t.EntryTime)
                    .Select(t => new TransactionHistoryItem
                    {
                        Id = t.TransactionNumber,
                        TicketNumber = t.TicketNumber,
                        PlateNumber = t.Vehicle != null ? t.Vehicle.VehicleNumber : string.Empty,
                        VehicleType = t.Vehicle != null ? t.Vehicle.VehicleType : string.Empty,
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
                    })
                    .ToListAsync();

                var model = new HistoryViewModel
                {
                    Transactions = transactions,
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                    TotalRevenue = transactions.Sum(t => t.Amount),
                    TotalTransactions = transactions.Count()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction history");
                return View(new HistoryViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Export(DateTime? startDate = null, DateTime? endDate = null, string format = "excel")
        {
            try
            {
                startDate = startDate ?? DateTime.Today;
                endDate = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var transactions = await _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .Include(t => t.ParkingSpace)
                    .Where(t => t.EntryTime.Date >= startDate.Value.Date && t.EntryTime.Date <= endDate.Value.Date)
                    .OrderByDescending(t => t.EntryTime)
                    .Select(t => new
                    {
                        VehicleNumber = t.Vehicle.VehicleNumber,
                        EntryTime = t.EntryTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        ExitTime = t.ExitTime.HasValue ? t.ExitTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "-",
                        Duration = t.ExitTime.HasValue ? (t.ExitTime.Value - t.EntryTime).ToString("hh\\:mm\\:ss") : "-",
                        Amount = t.TotalAmount,
                        PaymentStatus = t.PaymentStatus,
                        PaymentMethod = t.PaymentMethod,
                        PaymentTime = t.PaymentTime.HasValue ? t.PaymentTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "-",
                        EntryGate = t.EntryPoint,
                        ExitGate = t.ExitPoint,
                        EntryOperator = t.OperatorId,
                        ExitOperator = t.ExitOperatorId,
                        Status = t.Status
                    })
                    .ToListAsync();

                byte[] fileBytes;
                string fileName;
                string contentType;

                if (format.ToLower() == "pdf")
                {
                    fileBytes = GeneratePdf(transactions);
                    fileName = $"parking_history_{startDate.Value:yyyyMMdd}_{endDate.Value:yyyyMMdd}.pdf";
                    contentType = "application/pdf";
                }
                else
                {
                    fileBytes = GenerateExcel(transactions);
                    fileName = $"parking_history_{startDate.Value:yyyyMMdd}_{endDate.Value:yyyyMMdd}.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting transaction history");
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.ParkingTransactions
                .Include(t => t.Vehicle)
                .Include(t => t.ParkingSpace)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        private byte[] GenerateExcel(dynamic data)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Transaction History");
                
                // Add headers
                worksheet.Cells[1, 1].Value = "Vehicle Number";
                worksheet.Cells[1, 2].Value = "Entry Time";
                worksheet.Cells[1, 3].Value = "Exit Time";
                worksheet.Cells[1, 4].Value = "Duration";
                worksheet.Cells[1, 5].Value = "Amount";
                worksheet.Cells[1, 6].Value = "Status";

                // Add data
                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[row, 1].Value = item.VehicleNumber;
                    worksheet.Cells[row, 2].Value = item.EntryTime;
                    worksheet.Cells[row, 3].Value = item.ExitTime;
                    worksheet.Cells[row, 4].Value = item.Duration;
                    worksheet.Cells[row, 5].Value = item.Amount;
                    worksheet.Cells[row, 6].Value = item.Status;
                    row++;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }

        private byte[] GeneratePdf(dynamic data)
        {
            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4.Rotate());

                // Add title
                var title = new Paragraph("Transaction History Report")
                    .SetFontSize(18)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(title);
                document.Add(new Paragraph("\n"));

                // Create table
                var table = new Table(6).UseAllAvailableWidth();

                // Add headers
                var headers = new[] { "Vehicle Number", "Entry Time", "Exit Time", "Duration", "Amount", "Status" };
                foreach (var header in headers)
                {
                    table.AddCell(new Cell().Add(new Paragraph(header)).SetBold());
                }

                // Add data
                foreach (var item in data)
                {
                    table.AddCell(new Cell().Add(new Paragraph(item.VehicleNumber)));
                    table.AddCell(new Cell().Add(new Paragraph(item.EntryTime)));
                    table.AddCell(new Cell().Add(new Paragraph(item.ExitTime)));
                    table.AddCell(new Cell().Add(new Paragraph(item.Duration)));
                    table.AddCell(new Cell().Add(new Paragraph(item.Amount.ToString())));
                    table.AddCell(new Cell().Add(new Paragraph(item.Status)));
                }

                document.Add(table);
                document.Close();

                return ms.ToArray();
            }
        }
    }
} 