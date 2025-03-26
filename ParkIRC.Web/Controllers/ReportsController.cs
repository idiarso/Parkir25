using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Data;
using ParkIRC.Models;
using ParkIRC.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Pdf;
using System.IO;
using OfficeOpenXml;

namespace ParkIRC.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ApplicationDbContext context, ILogger<ReportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new ReportsViewModel
                {
                    DailyTransactions = await _context.ParkingTransactions
                        .Include(t => t.Vehicle)
                        .Where(t => t.EntryTime.Date == DateTime.Today)
                        .OrderByDescending(t => t.EntryTime)
                        .ToListAsync(),
                    MonthlyRevenue = await _context.ParkingTransactions
                        .Where(t => t.EntryTime >= DateTime.Today.AddMonths(-1))
                        .SumAsync(t => t.Amount),
                    VehicleTypeStats = await _context.ParkingTransactions
                        .Include(t => t.Vehicle)
                        .Where(t => t.EntryTime >= DateTime.Today.AddMonths(-1))
                        .GroupBy(t => t.Vehicle.VehicleType)
                        .Select(g => new VehicleTypeStats
                        {
                            VehicleType = g.Key,
                            Count = g.Count(),
                            TotalRevenue = g.Sum(t => t.Amount),
                            AverageTransaction = g.Average(t => t.Amount)
                        })
                        .OrderByDescending(g => g.TotalRevenue)
                        .ToListAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating reports");
                return StatusCode(500, "Error generating reports");
            }
        }

        public async Task<IActionResult> MonthlyReport(DateTime? date)
        {
            try
            {
                var reportDate = date ?? DateTime.Today;
                var startDate = new DateTime(reportDate.Year, reportDate.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var model = new ReportsViewModel
                {
                    DailyTransactions = await _context.ParkingTransactions
                        .Include(t => t.Vehicle)
                        .Where(t => t.EntryTime >= startDate && t.EntryTime <= endDate)
                        .OrderByDescending(t => t.EntryTime)
                        .ToListAsync(),
                    MonthlyRevenue = await _context.ParkingTransactions
                        .Where(t => t.EntryTime >= startDate && t.EntryTime <= endDate)
                        .SumAsync(t => t.Amount),
                    VehicleTypeStats = await _context.ParkingTransactions
                        .Include(t => t.Vehicle)
                        .Where(t => t.EntryTime >= startDate && t.EntryTime <= endDate)
                        .GroupBy(t => t.Vehicle.VehicleType)
                        .Select(g => new VehicleTypeStats
                        {
                            VehicleType = g.Key,
                            Count = g.Count(),
                            TotalRevenue = g.Sum(t => t.Amount),
                            AverageTransaction = g.Average(t => t.Amount)
                        })
                        .OrderByDescending(g => g.TotalRevenue)
                        .ToListAsync(),
                    CurrentDate = reportDate
                };

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating monthly report");
                return StatusCode(500, "Error generating monthly report");
            }
        }

        public async Task<IActionResult> ExportToPdf(DateTime? date)
        {
            try
            {
                var reportDate = date ?? DateTime.Today;
                var startDate = new DateTime(reportDate.Year, reportDate.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var transactions = await _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .Where(t => t.EntryTime >= startDate && t.EntryTime <= endDate)
                    .OrderByDescending(t => t.EntryTime)
                    .ToListAsync();

                var stats = await _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .Where(t => t.EntryTime >= startDate && t.EntryTime <= endDate)
                    .GroupBy(t => t.Vehicle.VehicleType)
                    .Select(g => new VehicleTypeStats
                    {
                        VehicleType = g.Key,
                        Count = g.Count(),
                        TotalRevenue = g.Sum(t => t.Amount),
                        AverageTransaction = g.Average(t => t.Amount)
                    })
                    .OrderByDescending(g => g.TotalRevenue)
                    .ToListAsync();

                var monthlyRevenue = await _context.ParkingTransactions
                    .Where(t => t.EntryTime >= startDate && t.EntryTime <= endDate)
                    .SumAsync(t => t.Amount);

                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Add title
                document.Add(new Paragraph($"Laporan Parkir - {reportDate:MMMM yyyy}")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(24));

                // Add monthly revenue
                document.Add(new Paragraph($"\nTotal Pendapatan Bulan Ini: Rp {string.Format("{0:N0}", monthlyRevenue)}")
                    .SetFontSize(16));

                // Add statistics table
                var statsTable = new Table(4);
                statsTable.AddHeaderCell("Tipe Kendaraan");
                statsTable.AddHeaderCell("Jumlah");
                statsTable.AddHeaderCell("Total Pendapatan");
                statsTable.AddHeaderCell("Rata-rata Transaksi");

                foreach (var stat in stats)
                {
                    statsTable.AddCell(stat.VehicleType);
                    statsTable.AddCell(stat.Count.ToString());
                    statsTable.AddCell($"Rp {string.Format("{0:N0}", stat.TotalRevenue)}");
                    statsTable.AddCell($"Rp {string.Format("{0:N0}", stat.AverageTransaction)}");
                }

                document.Add(statsTable);

                // Add transactions table
                document.Add(new Paragraph("\n\nTransaksi Harian").SetFontSize(16));
                var transactionsTable = new Table(6);
                transactionsTable.AddHeaderCell("No");
                transactionsTable.AddHeaderCell("Nomor Kendaraan");
                transactionsTable.AddHeaderCell("Tipe Kendaraan");
                transactionsTable.AddHeaderCell("Waktu Masuk");
                transactionsTable.AddHeaderCell("Waktu Keluar");
                transactionsTable.AddHeaderCell("Biaya");

                for (int i = 0; i < transactions.Count; i++)
                {
                    var transaction = transactions[i];
                    transactionsTable.AddCell((i + 1).ToString());
                    transactionsTable.AddCell(transaction.Vehicle.VehicleNumber);
                    transactionsTable.AddCell(transaction.Vehicle.VehicleType);
                    transactionsTable.AddCell(transaction.EntryTime.ToString("HH:mm"));
                    transactionsTable.AddCell(transaction.ExitTime?.ToString("HH:mm") ?? "Belum keluar");
                    transactionsTable.AddCell($"Rp {string.Format("{0:N0}", transaction.Amount)}");
                }

                document.Add(transactionsTable);
                document.Close();

                _logger.LogInformation($"Successfully generated PDF report for {reportDate:MMMM yyyy}");

                return File(stream.ToArray(), "application/pdf", $"parkir_report_{reportDate:yyyy_MM_dd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF report");
                return StatusCode(500, "Error generating PDF report");
            }
        }

        public async Task<IActionResult> ExportToExcel(DateTime? date)
        {
            try
            {
                var reportDate = date ?? DateTime.Today;
                var startDate = new DateTime(reportDate.Year, reportDate.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var transactions = await _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .Where(t => t.EntryTime >= startDate && t.EntryTime <= endDate)
                    .OrderByDescending(t => t.EntryTime)
                    .ToListAsync();

                var stats = await _context.ParkingTransactions
                    .Include(t => t.Vehicle)
                    .Where(t => t.EntryTime >= startDate && t.EntryTime <= endDate)
                    .GroupBy(t => t.Vehicle.VehicleType)
                    .Select(g => new VehicleTypeStats
                    {
                        VehicleType = g.Key,
                        Count = g.Count(),
                        TotalRevenue = g.Sum(t => t.Amount),
                        AverageTransaction = g.Average(t => t.Amount)
                    })
                    .OrderByDescending(g => g.TotalRevenue)
                    .ToListAsync();

                var monthlyRevenue = await _context.ParkingTransactions
                    .Where(t => t.EntryTime >= startDate && t.EntryTime <= endDate)
                    .SumAsync(t => t.Amount);

                using var stream = new MemoryStream();
                using var package = new ExcelPackage(stream);

                // Create statistics sheet
                var statsSheet = package.Workbook.Worksheets.Add("Statistik");
                statsSheet.Cells[1, 1].Value = $"Laporan Parkir - {reportDate:MMMM yyyy}";
                statsSheet.Cells[2, 1].Value = $"Total Pendapatan Bulan Ini: Rp {string.Format("{0:N0}", monthlyRevenue)}";

                // Add statistics table
                statsSheet.Cells[4, 1].Value = "Tipe Kendaraan";
                statsSheet.Cells[4, 2].Value = "Jumlah";
                statsSheet.Cells[4, 3].Value = "Total Pendapatan";
                statsSheet.Cells[4, 4].Value = "Rata-rata Transaksi";

                int row = 5;
                foreach (var stat in stats)
                {
                    statsSheet.Cells[row, 1].Value = stat.VehicleType;
                    statsSheet.Cells[row, 2].Value = stat.Count;
                    statsSheet.Cells[row, 3].Value = $"Rp {string.Format("{0:N0}", stat.TotalRevenue)}";
                    statsSheet.Cells[row, 4].Value = $"Rp {string.Format("{0:N0}", stat.AverageTransaction)}";
                    row++;
                }

                // Create transactions sheet
                var transactionsSheet = package.Workbook.Worksheets.Add("Transaksi");
                transactionsSheet.Cells[1, 1].Value = "No";
                transactionsSheet.Cells[1, 2].Value = "Nomor Kendaraan";
                transactionsSheet.Cells[1, 3].Value = "Tipe Kendaraan";
                transactionsSheet.Cells[1, 4].Value = "Waktu Masuk";
                transactionsSheet.Cells[1, 5].Value = "Waktu Keluar";
                transactionsSheet.Cells[1, 6].Value = "Biaya";

                for (int i = 0; i < transactions.Count; i++)
                {
                    var transaction = transactions[i];
                    transactionsSheet.Cells[i + 2, 1].Value = i + 1;
                    transactionsSheet.Cells[i + 2, 2].Value = transaction.Vehicle.VehicleNumber;
                    transactionsSheet.Cells[i + 2, 3].Value = transaction.Vehicle.VehicleType;
                    transactionsSheet.Cells[i + 2, 4].Value = transaction.EntryTime.ToString("HH:mm");
                    transactionsSheet.Cells[i + 2, 5].Value = transaction.ExitTime?.ToString("HH:mm") ?? "Belum keluar";
                    transactionsSheet.Cells[i + 2, 6].Value = $"Rp {string.Format("{0:N0}", transaction.Amount)}";
                }

                package.Save();

                _logger.LogInformation($"Successfully generated Excel report for {reportDate:MMMM yyyy}");

                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"parkir_report_{reportDate:yyyy_MM_dd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel report");
                return StatusCode(500, "Error generating Excel report");
            }
        }
    }
}
