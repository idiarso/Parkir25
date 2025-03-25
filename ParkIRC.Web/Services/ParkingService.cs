using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Data;
using ParkIRC.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using ParkIRC.Hubs;
using System.IO.Ports;
using System.Threading;

namespace ParkIRC.Services
{
    public class ParkingService : IParkingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ParkingService> _logger;
        private readonly IHubContext<GateHub> _gateHubContext;
        private readonly SerialPort _serialPort;
        private static readonly SemaphoreSlim _serialSemaphore = new SemaphoreSlim(1, 1);
        private readonly bool _useSerialSimulation;

        public ParkingService(
            ApplicationDbContext context, 
            ILogger<ParkingService> logger,
            IHubContext<GateHub> gateHubContext)
        {
            _context = context;
            _logger = logger;
            _gateHubContext = gateHubContext;
            
            // For now, we'll use simulation mode since we don't have the Arduino connected
            _useSerialSimulation = true;
            
            // In a real implementation, you would initialize the serial port:
            // _serialPort = new SerialPort("COM3", 9600);
            // try
            // {
            //     if (!_serialPort.IsOpen)
            //     {
            //         _serialPort.Open();
            //         _serialPort.DataReceived += SerialPort_DataReceived;
            //     }
            // }
            // catch (Exception ex)
            // {
            //     _logger.LogError(ex, "Failed to open serial port for gate communication");
            //     _useSerialSimulation = true;
            // }
        }

        public async Task<ParkingSpace> AssignParkingSpaceAsync(Vehicle vehicle)
        {
            return await AssignParkingSpace(vehicle);
        }

        public async Task<ParkingTransaction> ProcessExitAsync(string vehicleNumber, string paymentMethod)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.ParkingSpace)
                .FirstOrDefaultAsync(v => v.VehicleNumber == vehicleNumber && v.IsParked);

            if (vehicle == null)
            {
                throw new InvalidOperationException("Vehicle not found or not currently parked.");
            }

            var transaction = await ProcessCheckout(vehicle);
            transaction.PaymentMethod = paymentMethod;
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user != null;
        }

        public async Task<ParkingTransaction> ProcessCheckout(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle));
            }

            var exitTime = DateTime.UtcNow;
            var entryTime = vehicle.EntryTime ?? exitTime;
            var duration = exitTime - entryTime;
            var hours = Math.Ceiling(duration.TotalHours);
            var hourlyRate = vehicle.ParkingSpace?.HourlyRate ?? 0m;
            decimal totalAmount = (decimal)hours * hourlyRate;

            var transaction = new ParkingTransaction
            {
                VehicleId = vehicle.Id,
                ParkingSpaceId = vehicle.ParkingSpaceId ?? 0,
                TransactionNumber = GenerateTransactionNumber(),
                EntryTime = vehicle.EntryTime ?? exitTime,
                ExitTime = exitTime,
                Duration = (decimal)duration.TotalHours,
                TotalAmount = totalAmount,
                PaymentStatus = "Pending",
                Status = "Completed"
            };

            // Update vehicle status
            vehicle.IsParked = false;
            vehicle.ParkingSpaceId = null;
            vehicle.EntryTime = default;

            // Update parking space status
            if (vehicle.ParkingSpace != null)
            {
                vehicle.ParkingSpace.IsOccupied = false;
                vehicle.ParkingSpace.LastOccupiedTime = exitTime;
            }

            await _context.ParkingTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<ParkingSpace> AssignParkingSpace(Vehicle vehicle)
        {
            var availableSpace = await _context.ParkingSpaces
                .FirstOrDefaultAsync(p => !p.IsOccupied);

            if (availableSpace == null)
            {
                throw new InvalidOperationException("No parking spaces available");
            }

            availableSpace.IsOccupied = true;
            availableSpace.CurrentVehicle = vehicle;
            vehicle.ParkingSpace = availableSpace;
            vehicle.EntryTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return availableSpace;
        }

        public async Task<decimal> CalculateFee(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle));
            }

            var exitTime = DateTime.UtcNow;
            var entryTime = vehicle.EntryTime ?? exitTime;
            var duration = exitTime - entryTime;
            var hours = Math.Ceiling(duration.TotalHours);
            var hourlyRate = vehicle.ParkingSpace?.HourlyRate ?? 0m;
            return (decimal)hours * hourlyRate;
        }

        // New methods for Gate API
        public async Task<CommandResult> SendGateCommand(string gateId, string command)
        {
            _logger.LogInformation($"Sending gate command: {command} to gate: {gateId}");
            
            if (_useSerialSimulation)
            {
                // Simulate response for testing purposes
                await SimulateGateResponse(gateId, command);
                return new CommandResult { IsSuccess = true };
            }
            
            try
            {
                await _serialSemaphore.WaitAsync();
                
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    // Format command for Arduino with gate prefix
                    string fullCommand = $"{gateId.ToUpper()}:{command}\n";
                    _serialPort.WriteLine(fullCommand);
                    
                    // In real implementation, you would wait for a response
                    // For now, we'll simulate success
                    return new CommandResult { IsSuccess = true };
                }
                else
                {
                    return new CommandResult 
                    { 
                        IsSuccess = false,
                        ErrorMessage = "Serial port not available"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending command to gate {gateId}");
                return new CommandResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Failed to send command: {ex.Message}"
                };
            }
            finally
            {
                _serialSemaphore.Release();
            }
        }

        public async Task<(string GateState, string SensorState)> GetGateStatus(string gateId)
        {
            _logger.LogInformation($"Getting status for gate: {gateId}");
            
            if (_useSerialSimulation)
            {
                // Simulate random gate and sensor status for testing
                string gateState = new Random().Next(2) == 0 ? "CLOSED" : "OPEN";
                string sensorState = new Random().Next(2) == 0 ? "NO_VEHICLE" : "VEHICLE_DETECTED";
                
                // Notify via SignalR
                await _gateHubContext.Clients.All.SendAsync("GateStatusUpdated", new
                {
                    GateId = gateId,
                    Status = new
                    {
                        Gate = gateState,
                        Sensor = sensorState
                    },
                    LastUpdated = DateTime.UtcNow
                });
                
                return (gateState, sensorState);
            }
            
            try
            {
                await _serialSemaphore.WaitAsync();
                
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    // Request status from Arduino
                    _serialPort.WriteLine($"{gateId.ToUpper()}:STATUS\n");
                    
                    // In a real implementation, you would read the response
                    // For now, we'll simulate a response
                    return ("CLOSED", "NO_VEHICLE");
                }
                else
                {
                    return ("UNKNOWN", "UNKNOWN");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting status for gate {gateId}");
                return ("ERROR", "ERROR");
            }
            finally
            {
                _serialSemaphore.Release();
            }
        }

        private async Task SimulateGateResponse(string gateId, string command)
        {
            // Simulate delay
            await Task.Delay(500);
            
            string responseEvent = string.Empty;
            bool success = true;
            string message = "Command processed successfully";
            
            switch (command)
            {
                case "OPEN_GATE":
                    responseEvent = "GATE_OPENED";
                    break;
                case "CLOSE_GATE":
                    responseEvent = "GATE_CLOSED";
                    break;
                case "STATUS":
                    // Status is handled by GetGateStatus
                    return;
                default:
                    success = false;
                    message = "Unknown command";
                    break;
            }
            
            // Send the gate event via SignalR
            await _gateHubContext.Clients.All.SendAsync("ReceiveGateEvent", new
            {
                EventType = responseEvent,
                GateId = gateId,
                Timestamp = DateTime.UtcNow
            });
            
            // Send command result via SignalR
            await _gateHubContext.Clients.All.SendAsync("CommandResult", new
            {
                GateId = gateId,
                Command = command,
                Success = success,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }

        private static string GenerateTransactionNumber()
        {
            return "TRX-" + DateTime.Now.ToString("yyyyMMdd") + "-" + 
                   Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        }
        
        // Simulated handler for Arduino events that would come through the serial port
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // This would be implemented in a real scenario to handle incoming data from Arduino
            if (_serialPort == null) return;
            
            string data = _serialPort.ReadLine();
            _logger.LogInformation($"Received data from Arduino: {data}");
            
            // Parse the event and notify clients via SignalR
            ProcessArduinoEvent(data).ConfigureAwait(false);
        }
        
        private async Task ProcessArduinoEvent(string eventData)
        {
            if (string.IsNullOrEmpty(eventData)) return;
            
            string eventType = eventData;
            string gateId = "unknown";
            
            // Parse event format like "VEHICLE_DETECTED:ENTRY"
            if (eventData.Contains(":"))
            {
                var parts = eventData.Split(':');
                eventType = parts[0];
                gateId = parts[1].ToLower();
            }
            
            // Send event to all connected SignalR clients
            await _gateHubContext.Clients.All.SendAsync("ReceiveGateEvent", new
            {
                EventType = eventType,
                GateId = gateId,
                Timestamp = DateTime.UtcNow
            });
            
            // Depending on the event type, you might want to take additional actions
            // e.g., update database, trigger other systems, etc.
        }
    }
} 