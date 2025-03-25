using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ParkIRC.Services
{
    public interface IThermalScannerService
    {
        bool IsInitialized { get; }
        Task<bool> InitializeAsync();
        Task<string> ReadCodeAsync(int timeoutMs = 10000);
        Task<bool> TestConnectionAsync();
        Task<bool> StartContinuousScanAsync();
        Task<bool> StopContinuousScanAsync();
        event EventHandler<string> CodeScanned;
        event EventHandler<Exception> ScanError;
    }

    public class ThermalScannerService : IThermalScannerService, IDisposable
    {
        private readonly ILogger<ThermalScannerService> _logger;
        private readonly IConfiguration _configuration;
        private SerialPort _serialPort;
        private bool _isInitialized = false;
        private bool _isContinuousScanActive = false;
        private CancellationTokenSource _scanCancellationTokenSource;
        private StringBuilder _dataBuffer = new StringBuilder();

        public bool IsInitialized => _isInitialized;

        public event EventHandler<string> CodeScanned;
        public event EventHandler<Exception> ScanError;

        public ThermalScannerService(ILogger<ThermalScannerService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _serialPort = null;
            _scanCancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing thermal scanner service...");

                // Read configuration
                var portName = _configuration["ScannerSettings:PortName"];
                var baudRate = int.Parse(_configuration["ScannerSettings:BaudRate"] ?? "9600");
                var dataBits = int.Parse(_configuration["ScannerSettings:DataBits"] ?? "8");
                var parity = (Parity)Enum.Parse(typeof(Parity), _configuration["ScannerSettings:Parity"] ?? "None");
                var stopBits = (StopBits)Enum.Parse(typeof(StopBits), _configuration["ScannerSettings:StopBits"] ?? "One");
                var handshake = (Handshake)Enum.Parse(typeof(Handshake), _configuration["ScannerSettings:Handshake"] ?? "None");
                var autoConnect = bool.Parse(_configuration["ScannerSettings:AutoConnect"] ?? "true");

                // If port name is not specified and auto-connect is enabled, try to find a suitable port
                if (string.IsNullOrEmpty(portName) && autoConnect)
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
                        _logger.LogWarning("No serial ports found. Scanner will be unavailable.");
                        return false;
                    }
                }

                if (string.IsNullOrEmpty(portName))
                {
                    _logger.LogWarning("No port name specified and auto-connect is disabled. Scanner will be unavailable.");
                    return false;
                }

                _logger.LogInformation($"Initializing scanner on port {portName} with baud rate {baudRate}...");

                // Initialize the serial port
                _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
                {
                    Handshake = handshake,
                    ReadTimeout = int.Parse(_configuration["ScannerSettings:ReadTimeout"] ?? "1000")
                };

                _serialPort.DataReceived += SerialPort_DataReceived;

                try
                {
                    _serialPort.Open();
                    _logger.LogInformation("Serial port opened successfully.");
                    _isInitialized = true;
                    
                    // Start continuous scan if configured
                    if (bool.Parse(_configuration["ScannerSettings:ContinuousScan"] ?? "false"))
                    {
                        await StartContinuousScanAsync();
                    }
                    
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error opening serial port: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error initializing scanner: {ex.Message}");
                return false;
            }
        }

        public async Task<string> ReadCodeAsync(int timeoutMs = 10000)
        {
            _logger.LogInformation($"Waiting for code scan with timeout {timeoutMs}ms...");

            if (!_isInitialized || _serialPort == null || !_serialPort.IsOpen)
            {
                _logger.LogWarning("Scanner is not initialized or port is not open.");
                return string.Empty;
            }

            try
            {
                // Use a TaskCompletionSource to allow for cancellation
                var tcs = new TaskCompletionSource<string>();
                
                // Setup timeout
                using var cancellationTokenSource = new CancellationTokenSource(timeoutMs);
                cancellationTokenSource.Token.Register(() => tcs.TrySetResult(string.Empty), useSynchronizationContext: false);
                
                // Setup event handler for the code scan
                EventHandler<string> handler = null;
                handler = (sender, code) => {
                    tcs.TrySetResult(code);
                    CodeScanned -= handler;
                };
                
                CodeScanned += handler;
                
                // Wait for the scan or timeout
                var result = await tcs.Task;
                
                _logger.LogInformation(string.IsNullOrEmpty(result) 
                    ? "Read code timed out." 
                    : $"Code read: {result}");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reading code: {ex.Message}");
                ScanError?.Invoke(this, ex);
                return string.Empty;
            }
        }

        public Task<bool> TestConnectionAsync()
        {
            _logger.LogInformation("Testing scanner connection...");

            if (!_isInitialized || _serialPort == null || !_serialPort.IsOpen)
            {
                _logger.LogWarning("Scanner is not initialized or port is not open.");
                return Task.FromResult(false);
            }

            _logger.LogInformation("Scanner connection is active.");
            return Task.FromResult(true);
        }

        public async Task<bool> StartContinuousScanAsync()
        {
            _logger.LogInformation("Starting continuous scan...");

            if (!_isInitialized || _serialPort == null || !_serialPort.IsOpen)
            {
                _logger.LogWarning("Scanner is not initialized or port is not open.");
                return false;
            }

            if (_isContinuousScanActive)
            {
                _logger.LogWarning("Continuous scan is already active.");
                return true;
            }

            _isContinuousScanActive = true;
            _scanCancellationTokenSource = new CancellationTokenSource();

            try
            {
                // This is a placeholder. In a real implementation, we might need to send a command to the scanner
                // to start continuous scanning, depending on the scanner model.
                _logger.LogInformation("Continuous scan started.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting continuous scan: {ex.Message}");
                _isContinuousScanActive = false;
                return false;
            }
        }

        public async Task<bool> StopContinuousScanAsync()
        {
            _logger.LogInformation("Stopping continuous scan...");

            if (!_isContinuousScanActive)
            {
                _logger.LogWarning("Continuous scan is not active.");
                return true;
            }

            try
            {
                _scanCancellationTokenSource.Cancel();
                _isContinuousScanActive = false;

                // This is a placeholder. In a real implementation, we might need to send a command to the scanner
                // to stop continuous scanning, depending on the scanner model.
                _logger.LogInformation("Continuous scan stopped.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error stopping continuous scan: {ex.Message}");
                return false;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen)
                    return;

                // Read data from the serial port
                string data = _serialPort.ReadExisting();
                
                // Append to buffer
                _dataBuffer.Append(data);
                
                // Check if we have a complete code (ending with CR/LF)
                string bufferContent = _dataBuffer.ToString();
                if (bufferContent.Contains("\r") || bufferContent.Contains("\n"))
                {
                    // Split by newlines and process each line
                    string[] lines = bufferContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            _logger.LogInformation($"Scanned code: {line}");
                            CodeScanned?.Invoke(this, line.Trim());
                        }
                    }
                    
                    // Clear the buffer
                    _dataBuffer.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing scanner data: {ex.Message}");
                ScanError?.Invoke(this, ex);
            }
        }

        public void Dispose()
        {
            StopContinuousScanAsync().Wait();
            
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    _serialPort.Close();
                }
                _serialPort.Dispose();
                _serialPort = null;
            }
            
            _scanCancellationTokenSource?.Dispose();
        }
    }
} 