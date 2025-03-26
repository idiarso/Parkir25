using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Web.Data;
using ParkIRC.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using OfficeOpenXml;

namespace ParkIRC.Web.Controllers
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

                var query = _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .Include(t => t.ParkingSpace)
                    .Where(t => t.EntryTime.Date >= startDate.Value.Date && t.EntryTime.Date <= endDate.Value.Date);

                var transactions = await query
                    .OrderByDescending(t => t.EntryTime)
                    .Select(t => new TransactionHistoryItem
                    {
                        Id = t.TransactionNumber,
                        TicketNumber = t.TicketNumber,
                        PlateNumber = t.Vehicle != null ? t.Vehicle.VehicleNumber : string.Empty,
                        VehicleType = t.Vehicle != null ? t.Vehicle.VehicleType : string.Empty,
                        EntryTime = t.EntryTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        ExitTime = t.ExitTime.HasValue ? t.ExitTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "-",
                        Duration = t.ExitTime.HasValue ? (t.ExitTime.Value - t.EntryTime).ToString() : "-",
                        Amount = t.TotalAmount.ToString(),
                        PaymentStatus = t.PaymentStatus,
                        PaymentMethod = t.PaymentMethod,
                        PaymentTime = t.PaymentTime.HasValue ? t.PaymentTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "-",
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
                    TotalRevenue = transactions.Sum(t => decimal.Parse(t.Amount)),
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

                var query = _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .Include(t => t.ParkingSpace)
                    .Where(t => t.EntryTime.Date >= startDate.Value.Date && t.EntryTime.Date <= endDate.Value.Date);

                var transactions = await query
                    .OrderByDescending(t => t.EntryTime)
                    .Select(t => new TransactionHistoryItem
                    {
                        Id = t.TransactionNumber,
                        TicketNumber = t.TicketNumber,
                        PlateNumber = t.Vehicle != null ? t.Vehicle.VehicleNumber : string.Empty,
                        VehicleType = t.Vehicle != null ? t.Vehicle.VehicleType : string.Empty,
                        EntryTime = t.EntryTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        ExitTime = t.ExitTime.HasValue ? t.ExitTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "-",
                        Duration = t.ExitTime.HasValue ? (t.ExitTime.Value - t.EntryTime).ToString() : "-",
                        Amount = t.TotalAmount.ToString(),
                        PaymentStatus = t.PaymentStatus,
                        PaymentMethod = t.PaymentMethod,
                        PaymentTime = t.PaymentTime.HasValue ? t.PaymentTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "-",
                        EntryGate = t.EntryPoint,
                        ExitGate = t.ExitPoint,
                        EntryOperator = t.OperatorId,
                        ExitOperator = t.ExitOperatorId,
                        Status = t.Status,
                        IsPaid = t.PaymentStatus == "Paid"
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

        private byte[] GenerateExcel(List<TransactionHistoryItem> data)
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
                    worksheet.Cells[row, 1].Value = item.PlateNumber;
                    worksheet.Cells[row, 2].Value = item.EntryTime;
                    worksheet.Cells[row, 3].Value = item.ExitTime;
                    worksheet.Cells[row, 4].Value = item.Duration;
                    worksheet.Cells[row, 5].Value = decimal.Parse(item.Amount);
                    worksheet.Cells[row, 6].Value = item.Status;
                    row++;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }

        private byte[] GeneratePdf(List<TransactionHistoryItem> data)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf, PageSize.A4.Rotate());

            // Add title
            var title = new Paragraph("Transaction History Report")
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(title);
            document.Add(new Paragraph("\n"));

            // Create table
            var table = new Table(6).UseAllAvailableWidth();

            // Add headers
            var headers = new[] { "Vehicle Number", "Entry Time", "Exit Time", "Duration", "Amount", "Status" };
            foreach (var header in headers)
            {
                var cell = new Cell().Add(new Paragraph(header));
                cell.SetFont(PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD));
                table.AddCell(cell);
            }

            // Add data
            foreach (var item in data)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.PlateNumber)));
                table.AddCell(new Cell().Add(new Paragraph(item.EntryTime)));
                table.AddCell(new Cell().Add(new Paragraph(item.ExitTime ?? "-")));
                table.AddCell(new Cell().Add(new Paragraph(item.Duration)));
                table.AddCell(new Cell().Add(new Paragraph(item.Amount)));
                table.AddCell(new Cell().Add(new Paragraph(item.Status)));
            }

            document.Add(table);
            document.Close();

            return ms.ToArray();
        }
    }

    public class TransactionHistoryItem
    {
        public string Id { get; set; }
        public string TicketNumber { get; set; }
        public string PlateNumber { get; set; }
        public string VehicleType { get; set; }
        public string EntryTime { get; set; }
        public string? ExitTime { get; set; }
        public string Duration { get; set; }
        public string Amount { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string? PaymentTime { get; set; }
        public string EntryGate { get; set; }
        public string? ExitGate { get; set; }
        public string? EntryOperator { get; set; }
        public string? ExitOperator { get; set; }
        public string Status { get; set; }
        public bool IsPaid { get; set; }
    }

    public class HistoryViewModel
    {
        public List<TransactionHistoryItem> Transactions { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTransactions { get; set; }
    }
}