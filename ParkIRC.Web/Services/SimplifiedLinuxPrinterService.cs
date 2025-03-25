using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkIRC.Models;

namespace ParkIRC.Services
{
    public class SimplifiedLinuxPrinterService
    {
        private readonly ILogger<SimplifiedLinuxPrinterService> _logger;
        private readonly IConfiguration _configuration;
        private SerialPort _serialPort;
        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;

        public SimplifiedLinuxPrinterService(ILogger<SimplifiedLinuxPrinterService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _serialPort = null;
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing Linux thermal printer service...");

                // Read configuration
                var portName = _configuration["PrinterSettings:PortName"];
                var baudRate = int.Parse(_configuration["PrinterSettings:BaudRate"] ?? "9600");
                var dataBits = int.Parse(_configuration["PrinterSettings:DataBits"] ?? "8");
                var parity = (Parity)Enum.Parse(typeof(Parity), _configuration["PrinterSettings:Parity"] ?? "None");
                var stopBits = (StopBits)Enum.Parse(typeof(StopBits), _configuration["PrinterSettings:StopBits"] ?? "One");
                var handshake = (Handshake)Enum.Parse(typeof(Handshake), _configuration["PrinterSettings:Handshake"] ?? "None");

                // If port name is not specified, try to find a suitable port
                if (string.IsNullOrEmpty(portName))
                {
                    _logger.LogInformation("Port name not specified, searching for available ports...");
                    
                    var availablePorts = SerialPort.GetPortNames();
                    _logger.LogInformation($"Found {availablePorts.Length} serial ports: {string.Join(", ", availablePorts)}");
                    
                    if (availablePorts.Length > 0)
                    {
                        portName = availablePorts[0];
                        _logger.LogInformation($"Using port: {portName}");
                    }
                    else
                    {
                        _logger.LogWarning("No serial ports found. Using mock implementation.");
                        _isInitialized = true;
                        return true;
                    }
                }

                _logger.LogInformation($"Initializing printer on port {portName} with baud rate {baudRate}...");

                // Initialize the serial port
                _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
                {
                    Handshake = handshake,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };

                try
                {
                    _serialPort.Open();
                    _logger.LogInformation("Serial port opened successfully.");
                    
                    if (_configuration.GetValue<bool>("PrinterSettings:PrintTestOnStartup", false))
                    {
                        await TestPrinterAsync();
                    }
                    
                    _isInitialized = true;
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error opening serial port: {ex.Message}");
                    _logger.LogWarning("Using mock implementation.");
                    _isInitialized = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error initializing printer: {ex.Message}");
                _logger.LogWarning("Using mock implementation.");
                _isInitialized = true;
                return true;
            }
        }

        public async Task<bool> TestPrinterAsync()
        {
            _logger.LogInformation("Testing printer...");
            
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    string testMessage = 
                        "\x1B\x40" +  // Initialize printer
                        "\x1B\x45\x01" +  // Bold on
                        "ParkIRC Printer Test\n" +
                        "\x1B\x45\x00" +  // Bold off
                        "----------------------\n" +
                        "Test successful!\n" +
                        "Date: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n\n\n\n" +
                        "\x1D\x56\x41\x10";  // Cut paper
                    
                    await _serialPort.BaseStream.WriteAsync(Encoding.ASCII.GetBytes(testMessage));
                    _logger.LogInformation("Test message sent to printer.");
                    return true;
                }
                else
                {
                    _logger.LogInformation("Serial port not available, using mock implementation.");
                    _logger.LogInformation("TEST PRINT: ParkIRC Printer Test (MOCK)");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error testing printer: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PrintTicketAsync(Ticket ticket)
        {
            _logger.LogInformation($"Printing ticket: {ticket.TicketNumber}");
            
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    string companyName = _configuration["PrinterSettings:CompanyName"] ?? "ParkIRC Parking";
                    string companyAddress = _configuration["PrinterSettings:CompanyAddress"] ?? "Jl. Parking No. 123";
                    string companyPhone = _configuration["PrinterSettings:CompanyPhone"] ?? "021-12345678";
                    
                    string ticketContent = 
                        "\x1B\x40" +  // Initialize printer
                        "\x1B\x61\x01" +  // Center alignment
                        "\x1B\x45\x01" +  // Bold on
                        $"{companyName}\n" +
                        "\x1B\x45\x00" +  // Bold off
                        $"{companyAddress}\n" +
                        $"Tel: {companyPhone}\n" +
                        "----------------------\n" +
                        "\x1B\x61\x00" +  // Left alignment
                        $"Ticket: {ticket.TicketNumber}\n" +
                        $"Date  : {ticket.IssueTime:yyyy-MM-dd HH:mm:ss}\n" +
                        $"Vehicle: {ticket.VehicleNumber}\n" +
                        $"Type   : {ticket.VehicleType}\n" +
                        $"Space  : {ticket.ParkingSpace}\n" +
                        $"Entry  : {ticket.EntryTime:yyyy-MM-dd HH:mm:ss}\n" +
                        "----------------------\n" +
                        "\x1B\x61\x01" +  // Center alignment
                        "Thank you\n\n";
                    
                    // Add barcode if available
                    if (!string.IsNullOrEmpty(ticket.Barcode))
                    {
                        ticketContent += 
                            "\x1D\x68\x50" +  // Set barcode height
                            "\x1D\x77\x02" +  // Set barcode width
                            "\x1D\x6B\x04" +  // Print barcode (CODE39)
                            $"{ticket.Barcode.Length}" +  // Length of barcode data
                            $"{ticket.Barcode}" +  // Barcode data
                            "\n\n\n";  // Line feeds
                    }
                    else
                    {
                        ticketContent += "\n\n\n"; // Line feeds
                    }
                    
                    // Cut paper if enabled
                    if (_configuration.GetValue<bool>("PrinterSettings:AutoCut", true))
                    {
                        ticketContent += "\x1D\x56\x41\x10";  // Cut paper
                    }
                    
                    await _serialPort.BaseStream.WriteAsync(Encoding.ASCII.GetBytes(ticketContent));
                    _logger.LogInformation("Ticket printed successfully.");
                    return true;
                }
                else
                {
                    // Mock implementation
                    _logger.LogInformation("Serial port not available, using mock implementation.");
                    _logger.LogInformation("--- MOCK TICKET ---");
                    _logger.LogInformation($"Ticket: {ticket.TicketNumber}");
                    _logger.LogInformation($"Date  : {ticket.IssueTime:yyyy-MM-dd HH:mm:ss}");
                    _logger.LogInformation($"Vehicle: {ticket.VehicleNumber}");
                    _logger.LogInformation($"Type   : {ticket.VehicleType}");
                    _logger.LogInformation($"Space  : {ticket.ParkingSpace}");
                    _logger.LogInformation($"Entry  : {ticket.EntryTime:yyyy-MM-dd HH:mm:ss}");
                    _logger.LogInformation("--- END MOCK TICKET ---");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing ticket: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PrintReceiptAsync(string transactionNumber, string vehicleNumber, 
            DateTime entryTime, DateTime exitTime, string duration, decimal amount)
        {
            _logger.LogInformation($"Printing receipt: {transactionNumber}");
            
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    string companyName = _configuration["PrinterSettings:CompanyName"] ?? "ParkIRC Parking";
                    string companyAddress = _configuration["PrinterSettings:CompanyAddress"] ?? "Jl. Parking No. 123";
                    string companyPhone = _configuration["PrinterSettings:CompanyPhone"] ?? "021-12345678";
                    
                    string receiptContent = 
                        "\x1B\x40" +  // Initialize printer
                        "\x1B\x61\x01" +  // Center alignment
                        "\x1B\x45\x01" +  // Bold on
                        $"{companyName}\n" +
                        "\x1B\x45\x00" +  // Bold off
                        $"{companyAddress}\n" +
                        $"Tel: {companyPhone}\n" +
                        "----------------------\n" +
                        "\x1B\x61\x00" +  // Left alignment
                        $"Trans : {transactionNumber}\n" +
                        $"Vehicle: {vehicleNumber}\n" +
                        $"Entry  : {entryTime:yyyy-MM-dd HH:mm:ss}\n" +
                        $"Exit   : {exitTime:yyyy-MM-dd HH:mm:ss}\n" +
                        $"Duration: {duration}\n" +
                        "----------------------\n" +
                        "\x1B\x45\x01" +  // Bold on
                        $"Amount : Rp {amount:#,##0}\n" +
                        "\x1B\x45\x00" +  // Bold off
                        "----------------------\n" +
                        "\x1B\x61\x01" +  // Center alignment
                        "Thank you\n\n\n\n";  // Line feeds
                    
                    // Cut paper if enabled
                    if (_configuration.GetValue<bool>("PrinterSettings:AutoCut", true))
                    {
                        receiptContent += "\x1D\x56\x41\x10";  // Cut paper
                    }
                    
                    await _serialPort.BaseStream.WriteAsync(Encoding.ASCII.GetBytes(receiptContent));
                    _logger.LogInformation("Receipt printed successfully.");
                    return true;
                }
                else
                {
                    // Mock implementation
                    _logger.LogInformation("Serial port not available, using mock implementation.");
                    _logger.LogInformation("--- MOCK RECEIPT ---");
                    _logger.LogInformation($"Trans : {transactionNumber}");
                    _logger.LogInformation($"Vehicle: {vehicleNumber}");
                    _logger.LogInformation($"Entry  : {entryTime:yyyy-MM-dd HH:mm:ss}");
                    _logger.LogInformation($"Exit   : {exitTime:yyyy-MM-dd HH:mm:ss}");
                    _logger.LogInformation($"Duration: {duration}");
                    _logger.LogInformation($"Amount : Rp {amount:#,##0}");
                    _logger.LogInformation("--- END MOCK RECEIPT ---");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing receipt: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
            }
        }
    }
} 