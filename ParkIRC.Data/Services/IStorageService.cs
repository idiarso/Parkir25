using System.Threading.Tasks;
using ParkIRC.Data.Models;

namespace ParkIRC.Data.Services
{
    public interface IStorageService
    {
        Task StoreVehicleAsync(Vehicle vehicle);
        Task<ParkingSpace> GetAvailableSpaceAsync(string vehicleType);
    }
}
