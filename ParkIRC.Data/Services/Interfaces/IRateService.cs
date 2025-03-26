using System;
using System.Threading.Tasks;

namespace ParkIRC.Data.Services.Interfaces
{
    public interface IRateService
    {
        decimal CalculateFee(string vehicleType, TimeSpan duration);
        Task<decimal> CalculateFeeAsync(string vehicleType, TimeSpan duration);
    }
}
