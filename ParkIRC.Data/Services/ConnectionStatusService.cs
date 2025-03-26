using System.Threading.Tasks;

namespace ParkIRC.Data.Services
{
    public class ConnectionStatusService : IConnectionStatusService
    {
        public async Task<bool> IsCameraConnectedAsync(string cameraId)
        {
            // Implementation
            return true;
        }

        public async Task<bool> IsPrinterConnectedAsync(string printerId)
        {
            // Implementation
            return true;
        }

        public async Task<bool> IsGateConnectedAsync(string gateId)
        {
            // Implementation
            return true;
        }
    }
}
