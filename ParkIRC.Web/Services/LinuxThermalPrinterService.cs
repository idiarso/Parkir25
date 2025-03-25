using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkIRC.Models;
using ParkIRC.Extensions;

namespace ParkIRC.Services
{
    /// <summary>
    /// Service for direct communication with thermal printers on Linux via USB/Serial
    /// </summary>
    public class LinuxThermalPrinterService : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private System.IO.Ports.SerialPort _serialPort;
        private bool _isInitialized = false;
        private string _companyName = "SISTEM PARKIR RSI BANJARNEGARA";
        private string _address = "Jalan Raya Limpung - Bawang Km 08";
        private string _Kabupaten = "Kabupaten Banjarnegara";
        private string _footerText = "Terima Kasih!";
        
        // ESC/POS Commands
        private const byte ESC = 0x1B;
        private const byte GS = 0x1D;
        private const byte LF = 0x0A;
        private const byte CR = 0x0D;
        
        private readonly string _printerPort;
        private readonly int _baudRate;
        
        public LinuxThermalPrinterService(
            ILogger logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _printerPort = _configuration["Printer:Port"] ?? "/dev/ttyUSB0";
            _baudRate = int.TryParse(_configuration["Printer:BaudRate"], out int baudRate) ? baudRate : 9600;
        }
        
        public async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing thermal printer...");
                
                // Get settings from configuration
                var printerConfig = _configuration.GetSection("Printer");
                string portName = printerConfig["Connection"] ?? "";
                
                // Check if USB or Serial connection
                if (portName.StartsWith("usb://"))
                {
                    // For USB printers, we need to get the actual device path
                    string usbDevice = await GetUsbPrinterDeviceAsync(portName);
                    if (string.IsNullOrEmpty(usbDevice))
                    {
                        _logger.LogError("Could not find USB printer device path");
                        return false;
                    }
                    portName = usbDevice;
                }
                else if (string.IsNullOrEmpty(portName))
                {
                    // Try to find printer automatically
                    portName = await FindPrinterPortAsync();
                    if (string.IsNullOrEmpty(portName))
                    {
                        _logger.LogError("No printer port found or specified in configuration");
                        return false;
                    }
                }
                
                // Initialize serial port
                _serialPort = new SerialPort(portName)
                {
                    BaudRate = _baudRate,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };
                
                // Open serial port
                _serialPort.Open();
                _isInitialized = true;
                
                _logger.LogInformation($"Thermal printer initialized on port {portName}");
                
                // Reset printer to known state
                await ResetPrinterAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing thermal printer");
                return false;
            }
        }
        
        private async Task<string> GetUsbPrinterDeviceAsync(string usbPath)
        {
            try
            {
                // Parse the USB device identifiers from the path (e.g., "usb://EPSON/TM-T82")
                string[] parts = usbPath.Replace("usb://", "").Split('/');
                if (parts.Length < 2)
                {
                    return string.Empty;
                }
                
                string manufacturer = parts[0];
                string model = parts[1];
                
                // Use lsusb to find devices
                string output = await ExecuteCommandAsync("lsusb");
                string[] lines = output.Split('\n');
                
                foreach (var line in lines)
                {
                    if (line.Contains(manufacturer, StringComparison.OrdinalIgnoreCase) && 
                        line.Contains(model, StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract the bus and device IDs
                        string[] tokens = line.Split(' ');
                        if (tokens.Length >= 6)
                        {
                            string busId = tokens[1];
                            string deviceId = tokens[3].TrimEnd(':');
                            
                            // Check if /dev/usb/lp0 exists and is likely our printer
                            if (File.Exists("/dev/usb/lp0"))
                            {
                                return "/dev/usb/lp0";
                            }
                            
                            // Return the direct device path
                            return $"/dev/bus/usb/{busId}/{deviceId}";
                        }
                    }
                }
                
                // As a fallback, check for standard printer devices
                if (File.Exists("/dev/usb/lp0"))
                {
                    return "/dev/usb/lp0";
                }
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting USB printer device path");
                return string.Empty;
            }
        }
        
        private async Task<string> FindPrinterPortAsync()
        {
            try
            {
                // Check for USB printers first
                if (File.Exists("/dev/usb/lp0"))
                {
                    return "/dev/usb/lp0";
                }
                
                // Try to detect through lpstat
                string output = await ExecuteCommandAsync("lpstat -p");
                if (!string.IsNullOrEmpty(output) && output.Contains("printer"))
                {
                    // Parse the output to find the printer name
                    // This is just a basic implementation
                    string[] lines = output.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("printer "))
                        {
                            string printerName = line.Split(' ')[1];
                            // Use the printer via CUPS, to do this we would need a different approach
                            // For direct device access, we would check for serial ports
                            
                            // Try to find printer device through lsusb
                            return await GetDefaultUsbPrinterDeviceAsync();
                        }
                    }
                }
                
                // Check for serial ports
                string[] ports = SerialPort.GetPortNames();
                if (ports.Length > 0)
                {
                    return ports[0]; // Just use the first one
                }
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding printer port");
                return string.Empty;
            }
        }
        
        private async Task<string> GetDefaultUsbPrinterDeviceAsync()
        {
            try
            {
                // Check for USB printer devices
                string[] usbPrinterDevices = {
                    "/dev/usb/lp0",
                    "/dev/usb/lp1",
                    "/dev/usb/lp2",
                    "/dev/usb/lp3"
                };
                
                foreach (var device in usbPrinterDevices)
                {
                    if (File.Exists(device))
                    {
                        return device;
                    }
                }
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default USB printer device");
                return string.Empty;
            }
        }
        
        private async Task<string> ExecuteCommandAsync(string command)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using var process = new Process { StartInfo = processInfo };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing command: {command}");
                return string.Empty;
            }
        }
        
        private async Task ResetPrinterAsync()
        {
            if (!_isInitialized || _serialPort == null || !_serialPort.IsOpen)
            {
                return;
            }
            
            try
            {
                // Initialize printer
                byte[] initCommand = { ESC, (byte)'@' };
                await _serialPort.BaseStream.WriteAsync(initCommand, 0, initCommand.Length);
                
                // Set text justification to center
                byte[] centerCommand = { ESC, (byte)'a', 1 };
                await _serialPort.BaseStream.WriteAsync(centerCommand, 0, centerCommand.Length);
                
                await _serialPort.BaseStream.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting printer");
            }
        }
        
        public async Task<bool> PrintTicketAsync(ParkingTicket ticket)
        {
            if (!_isInitialized || _serialPort == null || !_serialPort.IsOpen)
            {
                if (!await InitializeAsync())
                {
                    return false;
                }
            }
            
            try
            {
                _logger.LogInformation($"Printing ticket {ticket.TicketNumber}...");
                
                // Create ESC/POS commands
                using MemoryStream ms = new MemoryStream();
                
                // Initialize printer
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'@');
                
                // Set text justification to center
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'a');
                ms.WriteByte(1);
                
                // Company name (double height, bold)
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'!');
                ms.WriteByte(16); // Double height
                WriteText(ms, _companyName);
                ms.WriteByte(LF);
                
                // Title (double height, bold)
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'!');
                ms.WriteByte(16); // Double height
                WriteText(ms, "TIKET PARKIR");
                ms.WriteByte(LF);
                
                // Address (normal)
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'!');
                ms.WriteByte(0); // Normal
                WriteText(ms, _address);
                ms.WriteByte(LF);
                
                WriteText(ms, _Kabupaten);
                ms.WriteByte(LF);
                ms.WriteByte(LF);
                
                // Set text justification to left
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'a');
                ms.WriteByte(0);
                
                // Ticket info
                WriteText(ms, $"NO. TIKET : {ticket.TicketNumber}");
                ms.WriteByte(LF);
                
                WriteText(ms, $"TANGGAL   : {ticket.IssueTime:dd/MM/yyyy}");
                ms.WriteByte(LF);
                
                WriteText(ms, $"JAM       : {ticket.IssueTime:HH:mm:ss}");
                ms.WriteByte(LF);
                
                if (!string.IsNullOrEmpty(ticket.VehicleNumber))
                {
                    WriteText(ms, $"KENDARAAN : {ticket.VehicleNumber}");
                    ms.WriteByte(LF);
                }
                
                WriteText(ms, $"JENIS     : {ticket.VehicleType}");
                ms.WriteByte(LF);
                ms.WriteByte(LF);
                
                // Barcode (if we have barcode data)
                if (!string.IsNullOrEmpty(ticket.BarcodeData))
                {
                    // Center align for barcode
                    ms.WriteByte(ESC);
                    ms.WriteByte((byte)'a');
                    ms.WriteByte(1);
                    
                    // Print barcode with CODE128
                    ms.WriteByte(GS);
                    ms.WriteByte((byte)'h');
                    ms.WriteByte(80); // Height
                    
                    ms.WriteByte(GS);
                    ms.WriteByte((byte)'w');
                    ms.WriteByte(2); // Width
                    
                    ms.WriteByte(GS);
                    ms.WriteByte((byte)'k');
                    ms.WriteByte(73); // CODE128
                    
                    byte[] barcodeBytes = Encoding.ASCII.GetBytes(ticket.BarcodeData);
                    ms.WriteByte((byte)barcodeBytes.Length);
                    ms.Write(barcodeBytes, 0, barcodeBytes.Length);
                    
                    ms.WriteByte(LF);
                    ms.WriteByte(LF);
                }
                
                // Footer
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'a');
                ms.WriteByte(1); // Center
                
                WriteText(ms, _footerText);
                ms.WriteByte(LF);
                ms.WriteByte(LF);
                
                // Cut paper
                ms.WriteByte(GS);
                ms.WriteByte((byte)'V');
                ms.WriteByte(66);
                ms.WriteByte(0);
                
                // Write the command buffer to the printer
                byte[] buffer = ms.ToArray();
                await _serialPort.BaseStream.WriteAsync(buffer, 0, buffer.Length);
                await _serialPort.BaseStream.FlushAsync();
                
                _logger.LogInformation($"Ticket {ticket.TicketNumber} printed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing ticket {ticket.TicketNumber}");
                return false;
            }
        }
        
        public async Task<bool> PrintReceiptAsync(ParkingTransaction transaction)
        {
            if (!_isInitialized || _serialPort == null || !_serialPort.IsOpen)
            {
                if (!await InitializeAsync())
                {
                    return false;
                }
            }
            
            try
            {
                _logger.LogInformation($"Printing receipt for transaction {transaction.TransactionNumber}...");
                
                // Create ESC/POS commands
                using MemoryStream ms = new MemoryStream();
                
                // Initialize printer
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'@');
                
                // Set text justification to center
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'a');
                ms.WriteByte(1);
                
                // Company name (double height, bold)
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'!');
                ms.WriteByte(16); // Double height
                WriteText(ms, _companyName);
                ms.WriteByte(LF);
                
                // Title (double height, bold)
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'!');
                ms.WriteByte(16); // Double height
                WriteText(ms, "STRUK PARKIR");
                ms.WriteByte(LF);
                
                // Address (normal)
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'!');
                ms.WriteByte(0); // Normal
                WriteText(ms, _address);
                ms.WriteByte(LF);
                
                WriteText(ms, _Kabupaten);
                ms.WriteByte(LF);
                ms.WriteByte(LF);
                
                // Set text justification to left
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'a');
                ms.WriteByte(0);
                
                // Transaction info
                WriteText(ms, $"NO. TRANSAKSI : {transaction.TransactionNumber}");
                ms.WriteByte(LF);
                
                if (transaction.Vehicle != null && !string.IsNullOrEmpty(transaction.Vehicle.VehicleNumber))
                {
                    WriteText(ms, $"KENDARAAN     : {transaction.Vehicle.VehicleNumber}");
                    ms.WriteByte(LF);
                }
                
                WriteText(ms, $"MASUK         : {transaction.EntryTime:dd/MM/yyyy HH:mm}");
                ms.WriteByte(LF);
                
                WriteText(ms, $"KELUAR        : {transaction.ExitTime:dd/MM/yyyy HH:mm}");
                ms.WriteByte(LF);
                
                // Duration (if exit time is set)
                if (transaction.ExitTime.HasValue)
                {
                    TimeSpan duration = transaction.ExitTime.Value - transaction.EntryTime;
                    WriteText(ms, $"DURASI        : {duration.Hours:00}:{duration.Minutes:00}");
                    ms.WriteByte(LF);
                }
                
                ms.WriteByte(LF);
                
                // Total (emphasized)
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'E');
                ms.WriteByte(1); // Emphasized on
                
                WriteText(ms, $"TOTAL         : {transaction.TotalAmount.ToRupiah()}");
                ms.WriteByte(LF);
                
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'E');
                ms.WriteByte(0); // Emphasized off
                
                ms.WriteByte(LF);
                
                // Footer
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'a');
                ms.WriteByte(1); // Center
                
                WriteText(ms, _footerText);
                ms.WriteByte(LF);
                ms.WriteByte(LF);
                
                // Cut paper
                ms.WriteByte(GS);
                ms.WriteByte((byte)'V');
                ms.WriteByte(66);
                ms.WriteByte(0);
                
                // Write the command buffer to the printer
                byte[] buffer = ms.ToArray();
                await _serialPort.BaseStream.WriteAsync(buffer, 0, buffer.Length);
                await _serialPort.BaseStream.FlushAsync();
                
                _logger.LogInformation($"Receipt for transaction {transaction.TransactionNumber} printed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing receipt for transaction {transaction.TransactionNumber}");
                return false;
            }
        }
        
        private void WriteText(MemoryStream ms, string text)
        {
            byte[] textBytes = Encoding.GetEncoding("ASCII", new EncoderReplacementFallback("?"), new DecoderReplacementFallback("?")).GetBytes(text);
            ms.Write(textBytes, 0, textBytes.Length);
        }
        
        public async Task<bool> TestPrinterAsync()
        {
            if (!_isInitialized || _serialPort == null || !_serialPort.IsOpen)
            {
                if (!await InitializeAsync())
                {
                    return false;
                }
            }
            
            try
            {
                // Create ESC/POS commands
                using MemoryStream ms = new MemoryStream();
                
                // Initialize printer
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'@');
                
                // Set text justification to center
                ms.WriteByte(ESC);
                ms.WriteByte((byte)'a');
                ms.WriteByte(1);
                
                // Test message
                WriteText(ms, "=== PRINTER TEST ===");
                ms.WriteByte(LF);
                ms.WriteByte(LF);
                
                WriteText(ms, "ParkIRC Thermal Printer Test");
                ms.WriteByte(LF);
                
                WriteText(ms, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                ms.WriteByte(LF);
                ms.WriteByte(LF);
                
                // Cut paper
                ms.WriteByte(GS);
                ms.WriteByte((byte)'V');
                ms.WriteByte(66);
                ms.WriteByte(0);
                
                // Write the command buffer to the printer
                byte[] buffer = ms.ToArray();
                await _serialPort.BaseStream.WriteAsync(buffer, 0, buffer.Length);
                await _serialPort.BaseStream.FlushAsync();
                
                _logger.LogInformation("Printer test successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing printer");
                return false;
            }
        }
        
        public void Dispose()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                    _serialPort = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing thermal printer service");
            }
        }
    }
} 