using System.Threading.Tasks;
using ParkIRC.Data.Models;

namespace ParkIRC.Data.Services
{
    public interface IRateService
    {
        Task<Rate> GetActiveRateAsync(string vehicleType);
        Task<decimal> CalculateFeeAsync(string vehicleType, TimeSpan duration);
        Task UpdateRateAsync(Rate rate);
        Task<bool> IsRateActiveAsync(string vehicleType);
    }
}
