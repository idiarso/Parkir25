using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Data.Models;
using ParkIRC.Data.Data;
using ParkIRC.Data.Services.Interfaces;

namespace ParkIRC.Data.Services
{
    public class RateService : IRateService
    {
        private readonly ApplicationDbContext _context;

        public RateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Rate> GetActiveRateAsync(string vehicleType)
        {
            return await _context.Rates
                .FirstOrDefaultAsync(r => r.VehicleType == vehicleType && r.IsActive);
        }

        public decimal CalculateFee(string vehicleType, TimeSpan duration)
        {
            var rate = _context.Rates
                .FirstOrDefault(r => r.VehicleType == vehicleType && r.IsActive);

            if (rate == null)
                return 0;

            var hours = (int)Math.Ceiling(duration.TotalHours);
            return rate.BaseFee + (hours * rate.HourlyRate);
        }

        public async Task<decimal> CalculateFeeAsync(string vehicleType, TimeSpan duration)
        {
            var rate = await _context.Rates
                .FirstOrDefaultAsync(r => r.VehicleType == vehicleType && r.IsActive);

            if (rate == null)
                return 0;

            var hours = (int)Math.Ceiling(duration.TotalHours);
            return rate.BaseFee + (hours * rate.HourlyRate);
        }

        public async Task UpdateRateAsync(Rate rate)
        {
            _context.Rates.Update(rate);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsRateActiveAsync(string vehicleType)
        {
            return await _context.Rates
                .AnyAsync(r => r.VehicleType == vehicleType && r.IsActive);
        }
    }
}
