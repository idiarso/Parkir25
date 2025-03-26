using System.Threading.Tasks;

namespace ParkIRC.Data.Services.Interfaces
{
    public interface ICameraService
    {
        Task<string> CaptureImageAsync(string cameraId);
    }
}
