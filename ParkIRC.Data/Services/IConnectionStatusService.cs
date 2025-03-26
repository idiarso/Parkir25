using System.Threading.Tasks;

namespace ParkIRC.Data.Services
{
    public interface IConnectionStatusService
    {
        Task<bool> IsCameraConnectedAsync(string cameraId);
        Task<bool> IsPrinterConnectedAsync(string printerId);
        Task<bool> IsGateConnectedAsync(string gateId);
    }
}
