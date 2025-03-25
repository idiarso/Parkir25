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
        Task<bool> InitializeAsync();
        Task<string> ReadCodeAsync(int timeoutMs = 10000);
        Task<bool> TestConnectionAsync();
        bool IsConnected { get; }
        event EventHandler<ScannerEventArgs> CodeScanned;
        event EventHandler<ScannerEventArgs> ScannerError;
        Task StartContinuousScanAsync(CancellationToken cancellationToken);
        void StopContinuousScan();
    }

    public class ThermalScannerService : IThermalScannerService, IDisposable
    {
        private readonly ILogger<ThermalScannerService> _logger;
        private readonly IConfiguration _configuration;
        private SerialPort _serialPort;
        private bool _isInitialized = false;
        private CancellationTokenSource _scanCts;
        private StringBuilder _dataBuffer = new StringBuilder();
        private readonly object _lockObject = new object();

        // Events
        public event EventHandler<ScannerEventArgs> CodeScanned;
        public event EventHandler<ScannerEventArgs> ScannerError;

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public ThermalScannerService(ILogger<ThermalScannerService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _scanCts = new CancellationTokenSource();
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing thermal scanner...");

                // Get settings from configuration
                var scannerConfig = _configuration.GetSection("ScannerSettings");
                string portName = scannerConfig["PortName"] ?? FindScannerPort();
                int baudRate = int.Parse(scannerConfig["BaudRate"] ?? "9600");
                
                if (string.IsNullOrEmpty(portName))
                {
                    _logger.LogError("No scanner port found or specified in configuration");
                    OnScannerError("No scanner port found");
                    return false;
                }

                // Initialize serial port
                _serialPort = new SerialPort(portName, baudRate)
                {
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };

                // Set up serial port data received event handler
                _serialPort.DataReceived += SerialPort_DataReceived;

                // Open serial port
                _serialPort.Open();
                _isInitialized = true;

                // Let's test if it's really working
                bool testResult = await TestConnectionAsync();
                if (!testResult)
                {
                    _logger.LogWarning("Scanner initialization succeeded but test failed");
                }

                _logger.LogInformation($"Thermal scanner initialized on port {portName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing thermal scanner");
                OnScannerError($"Initialization error: {ex.Message}");
                return false;
            }
        }

        private string FindScannerPort()
        {
            try
            {
                // Get list of available serial ports
                string[] ports = SerialPort.GetPortNames();
                
                if (ports.Length == 0)
                {
                    _logger.LogWarning("No serial ports found");
                    return string.Empty;
                }

                // Try to detect the scanner by testing each port
                foreach (string port in ports)
                {
                    try
                    {
                        using (var testPort = new SerialPort(port, 9600))
                        {
                            testPort.Open();
                            
                            // If we can open the port, it might be our scanner
                            // We could add more sophisticated detection here
                            
                            testPort.Close();
                            _logger.LogInformation($"Found potential scanner port: {port}");
                            return port;
                        }
                    }
                    catch
                    {
                        // This port is either in use or not available
                        continue;
                    }
                }

                // If we can't determine the scanner port, just return the first available port
                if (ports.Length > 0)
                {
                    _logger.LogInformation($"Using first available port: {ports[0]}");
                    return ports[0];
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding scanner port");
                return string.Empty;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    return;
                }

                // Read data from the serial port
                string data = _serialPort.ReadExisting();
                
                lock (_lockObject)
                {
                    _dataBuffer.Append(data);
                    
                    // Check if we have a complete code (usually ends with CR, LF, or both)
                    string bufferStr = _dataBuffer.ToString();
                    if (bufferStr.Contains("\r") || bufferStr.Contains("\n"))
                    {
                        // Process the code
                        string code = bufferStr.Trim();
                        _dataBuffer.Clear();
                        
                        // Raise event
                        OnCodeScanned(code);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving data from scanner");
                OnScannerError($"Data receive error: {ex.Message}");
            }
        }

        public async Task<string> ReadCodeAsync(int timeoutMs = 10000)
        {
            if (!_isInitialized || _serialPort == null || !_serialPort.IsOpen)
            {
                await InitializeAsync();
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("Scanner not initialized");
                }
            }

            try
            {
                _logger.LogInformation("Waiting for barcode scan...");
                
                // Clear any existing data
                lock (_lockObject)
                {
                    _dataBuffer.Clear();
                }

                // Create task completion source for async waiting
                var tcs = new TaskCompletionSource<string>();
                
                // Event handler for getting scan result
                void CodeScannedHandler(object sender, ScannerEventArgs e)
                {
                    if (e.Success && !string.IsNullOrEmpty(e.Code))
                    {
                        tcs.TrySetResult(e.Code);
                    }
                    else
                    {
                        tcs.TrySetException(new Exception("Error scanning code: " + e.Message));
                    }
                }

                // Subscribe to event
                CodeScanned += CodeScannedHandler;
                
                try
                {
                    // Wait for scan with timeout
                    using (var cts = new CancellationTokenSource(timeoutMs))
                    {
                        var timeoutTask = Task.Delay(timeoutMs, cts.Token);
                        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);
                        
                        if (completedTask == timeoutTask)
                        {
                            throw new TimeoutException("Scanner timeout: No code scanned");
                        }
                        
                        return await tcs.Task;
                    }
                }
                finally
                {
                    // Unsubscribe from event
                    CodeScanned -= CodeScannedHandler;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading code from scanner");
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            if (!_isInitialized || _serialPort == null)
            {
                await InitializeAsync();
            }

            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    // We could send a test command here if the scanner supports it
                    // For now, we just check if the port is open
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing scanner connection");
                return false;
            }
        }

        public async Task StartContinuousScanAsync(CancellationToken cancellationToken)
        {
            if (!_isInitialized || _serialPort == null || !_serialPort.IsOpen)
            {
                await InitializeAsync();
                if (!_isInitialized)
                {
                    OnScannerError("Cannot start continuous scan - scanner not initialized");
                    return;
                }
            }

            try
            {
                // Create a linked token that will cancel if either the passed token or our internal token cancels
                _scanCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var linkedToken = _scanCts.Token;

                _logger.LogInformation("Starting continuous scan mode");
                
                // Run a background task that just keeps the scanner active
                await Task.Factory.StartNew(async () => 
                {
                    try 
                    {
                        while (!linkedToken.IsCancellationRequested) 
                        {
                            // Keep the service alive
                            await Task.Delay(1000, linkedToken);
                        }
                    }
                    catch (OperationCanceledException) 
                    {
                        // Expected when cancellation is requested
                        _logger.LogInformation("Continuous scanning stopped");
                    }
                    catch (Exception ex) 
                    {
                        _logger.LogError(ex, "Error in continuous scan loop");
                        OnScannerError($"Continuous scan error: {ex.Message}");
                    }
                }, linkedToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting continuous scan");
                OnScannerError($"Failed to start continuous scan: {ex.Message}");
            }
        }

        public void StopContinuousScan()
        {
            try
            {
                _scanCts?.Cancel();
                _logger.LogInformation("Continuous scan stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping continuous scan");
            }
        }

        protected virtual void OnCodeScanned(string code)
        {
            try
            {
                _logger.LogInformation($"Code scanned: {code}");
                CodeScanned?.Invoke(this, new ScannerEventArgs
                {
                    Success = true,
                    Code = code,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnCodeScanned");
            }
        }

        protected virtual void OnScannerError(string message)
        {
            try
            {
                _logger.LogWarning($"Scanner error: {message}");
                ScannerError?.Invoke(this, new ScannerEventArgs
                {
                    Success = false,
                    Message = message,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnScannerError");
            }
        }

        public void Dispose()
        {
            try
            {
                StopContinuousScan();
                
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    _serialPort.Close();
                    _serialPort.Dispose();
                    _serialPort = null;
                }
                
                _scanCts?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing scanner service");
            }
        }
    }

    public class ScannerEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
} 