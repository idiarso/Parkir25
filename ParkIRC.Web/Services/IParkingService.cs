using ParkIRC.Models;
using System.Threading.Tasks;

namespace ParkIRC.Services
{
    public interface IParkingService
    {
        Task<ParkingSpace> AssignParkingSpaceAsync(Vehicle vehicle);
        Task<ParkingTransaction> ProcessExitAsync(string vehicleNumber, string paymentMethod);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<ParkingTransaction> ProcessCheckout(Vehicle vehicle);
        Task<ParkingSpace> AssignParkingSpace(Vehicle vehicle);
        Task<decimal> CalculateFee(Vehicle vehicle);
        
        // New methods for Gate API
        Task<CommandResult> SendGateCommand(string gateId, string command);
        Task<(string GateState, string SensorState)> GetGateStatus(string gateId);
    }
    
    public class CommandResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
} 