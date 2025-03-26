using System;
using System.Threading.Tasks;
using ParkIRC.Data.Services.Interfaces;

namespace ParkIRC.Data.Services
{
    public class CameraService : ICameraService
    {
        private readonly IConnectionStatusService _connectionStatusService;

        public CameraService(IConnectionStatusService connectionStatusService)
        {
            _connectionStatusService = connectionStatusService;
        }

        public async Task<string> CaptureImageAsync(string cameraId)
        {
            if (await _connectionStatusService.IsCameraConnectedAsync(cameraId))
            {
                // Implementation for capturing image
                return "image_path";
            }
            return null;
        }

        public async Task<string> GetCameraStatusAsync(string cameraId)
        {
            var isConnected = await _connectionStatusService.IsCameraConnectedAsync(cameraId);
            return isConnected ? "Connected" : "Disconnected";
        }
    }
}
