using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ParkIRC.Hubs
{
    public class GateHub : Hub
    {
        private readonly ILogger<GateHub> _logger;

        public GateHub(ILogger<GateHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected to GateHub: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected from GateHub: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendGateEvent(string gateId, string eventType, object data)
        {
            await Clients.All.SendAsync("ReceiveGateEvent", new
            {
                EventType = eventType,
                GateId = gateId,
                Timestamp = DateTime.UtcNow,
                Data = data
            });
        }

        public async Task NotifyGateStatus(string gateId, string gateState, string sensorState)
        {
            await Clients.All.SendAsync("GateStatusUpdated", new
            {
                GateId = gateId,
                Status = new
                {
                    Gate = gateState,
                    Sensor = sensorState
                },
                LastUpdated = DateTime.UtcNow
            });
        }

        public async Task NotifyVehicleDetected(string gateId)
        {
            await Clients.All.SendAsync("VehicleDetected", new
            {
                GateId = gateId,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyCommandResult(string gateId, string command, bool success, string message)
        {
            await Clients.All.SendAsync("CommandResult", new
            {
                GateId = gateId,
                Command = command,
                Success = success,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
} 