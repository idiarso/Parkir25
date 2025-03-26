using System.Threading.Tasks;
using ParkIRC.Data.Models;

namespace ParkIRC.Data.Services
{
    public interface IParkingService
    {
        Task<ParkingSpace> AssignParkingSpaceAsync(Vehicle vehicle);
        ParkingSpace AssignParkingSpace(Vehicle vehicle);
        void ProcessCheckout(Vehicle vehicle);
        decimal CalculateFee(Vehicle vehicle);
        Task<decimal> CalculateFeeAsync(Vehicle vehicle);
        Task<ParkingSpace> GetAvailableSpaceAsync(string vehicleType);
        Task<bool> IsSpaceAvailableAsync(string vehicleType);
    }
}
