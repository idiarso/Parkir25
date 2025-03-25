using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkIRC.Test.Models;

namespace ParkIRC.Test
{
    public class SimplifiedLinuxPrinterService
    {
        private readonly ILogger<SimplifiedLinuxPrinterService> _logger;
        private readonly IConfiguration _configuration;
        private bool _isInitialized;

        public SimplifiedLinuxPrinterService(ILogger<SimplifiedLinuxPrinterService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _isInitialized = false;
        }

        public bool IsInitialized => _isInitialized;

        public async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing simplified Linux printer service...");
                // Simulasi inisialisasi
                await Task.Delay(500);
                _isInitialized = true;
                _logger.LogInformation("Printer service initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing printer service");
                return false;
            }
        }

        public async Task<bool> TestPrinterAsync()
        {
            if (!_isInitialized)
            {
                _logger.LogWarning("Printer not initialized");
                return false;
            }

            try
            {
                _logger.LogInformation("Testing printer...");
                // Simulasi print test
                await Task.Delay(500);
                _logger.LogInformation("Test print successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing printer");
                return false;
            }
        }

        public async Task<bool> PrintTicketAsync(Ticket ticket)
        {
            if (!_isInitialized)
            {
                _logger.LogWarning("Printer not initialized");
                return false;
            }

            try
            {
                _logger.LogInformation($"Printing ticket: {ticket.TicketNumber}");
                
                // Format tiket untuk printer
                string ticketContent = FormatTicket(ticket);
                
                // Simulasi print
                await Task.Delay(1000);
                
                _logger.LogInformation("Ticket printed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing ticket");
                return false;
            }
        }

        public async Task<bool> PrintReceiptAsync(
            string transactionNumber,
            string vehicleNumber,
            DateTime entryTime,
            DateTime exitTime,
            string duration,
            decimal amount)
        {
            if (!_isInitialized)
            {
                _logger.LogWarning("Printer not initialized");
                return false;
            }

            try
            {
                _logger.LogInformation($"Printing receipt: {transactionNumber}");
                
                // Format receipt untuk printer
                string receiptContent = FormatReceipt(
                    transactionNumber, 
                    vehicleNumber, 
                    entryTime, 
                    exitTime, 
                    duration, 
                    amount);
                
                // Simulasi print
                await Task.Delay(1000);
                
                _logger.LogInformation("Receipt printed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing receipt");
                return false;
            }
        }

        private string FormatTicket(Ticket ticket)
        {
            return $@"
==================================
        TIKET PARKIR
==================================
Ticket No.: {ticket.TicketNumber}
Tanggal   : {ticket.IssueTime:dd/MM/yyyy HH:mm}
Kendaraan : {ticket.VehicleNumber}
Jenis     : {ticket.VehicleType}
Lokasi    : {ticket.ParkingSpace}
==================================
         Terima Kasih
==================================
";
        }

        private string FormatReceipt(
            string transactionNumber,
            string vehicleNumber,
            DateTime entryTime,
            DateTime exitTime,
            string duration,
            decimal amount)
        {
            return $@"
==================================
        STRUK PARKIR
==================================
No. Transaksi: {transactionNumber}
Kendaraan    : {vehicleNumber}
Masuk        : {entryTime:dd/MM/yyyy HH:mm}
Keluar       : {exitTime:dd/MM/yyyy HH:mm}
Durasi       : {duration}
Total        : Rp {amount:N0}
==================================
         Terima Kasih
==================================
";
        }
    }
} 