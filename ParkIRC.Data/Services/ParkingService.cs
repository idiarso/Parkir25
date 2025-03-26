using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Data.Data;
using ParkIRC.Data.Models;

namespace ParkIRC.Data.Services
{
    public class ParkingService : IParkingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRateService _rateService;

        public ParkingService(ApplicationDbContext context, IRateService rateService)
        {
            _context = context;
            _rateService = rateService;
        }

        public async Task<ParkingSpace> AssignParkingSpaceAsync(Vehicle vehicle)
        {
            var availableSpace = await _context.ParkingSpaces
                .FirstOrDefaultAsync(s => s.IsAvailable && s.VehicleType == vehicle.VehicleType);

            if (availableSpace != null)
            {
                availableSpace.IsAvailable = false;
                vehicle.ParkingSpaceId = availableSpace.Id;
                await _context.SaveChangesAsync();
            }

            return availableSpace;
        }

        public ParkingSpace AssignParkingSpace(Vehicle vehicle)
        {
            var availableSpace = _context.ParkingSpaces
                .FirstOrDefault(s => s.IsAvailable && s.VehicleType == vehicle.VehicleType);

            if (availableSpace != null)
            {
                availableSpace.IsAvailable = false;
                vehicle.ParkingSpaceId = availableSpace.Id;
                _context.SaveChanges();
            }

            return availableSpace;
        }

        public async Task<bool> CheckInVehicleAsync(Vehicle vehicle)
        {
            var parkingSpace = await _context.ParkingSpaces
                .FirstOrDefaultAsync(s => s.Id == vehicle.ParkingSpaceId);

            if (parkingSpace != null && parkingSpace.IsAvailable)
            {
                parkingSpace.IsAvailable = false;
                vehicle.EntryTime = DateTime.Now;
                await _context.Vehicles.AddAsync(vehicle);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public void ProcessCheckout(Vehicle vehicle)
        {
            var parkingSpace = _context.ParkingSpaces
                .FirstOrDefault(s => s.Id == vehicle.ParkingSpaceId);

            if (parkingSpace != null && !parkingSpace.IsAvailable)
            {
                parkingSpace.IsAvailable = true;
                vehicle.ExitTime = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public decimal CalculateFee(Vehicle vehicle)
        {
            if (vehicle.EntryTime == null || vehicle.ExitTime == null)
                return 0;

            var duration = vehicle.ExitTime.Value - vehicle.EntryTime;
            return _rateService.CalculateFeeAsync(vehicle.VehicleType, duration).Result;
        }

        public async Task<decimal> CalculateFeeAsync(Vehicle vehicle)
        {
            if (vehicle.EntryTime == null || vehicle.ExitTime == null)
                return 0;

            var duration = vehicle.ExitTime.Value - vehicle.EntryTime;
            return await _rateService.CalculateFeeAsync(vehicle.VehicleType, duration);
        }

        public async Task<ParkingSpace> GetAvailableSpaceAsync(string vehicleType)
        {
            return await _context.ParkingSpaces
                .FirstOrDefaultAsync(s => s.IsAvailable && s.VehicleType == vehicleType);
        }

        public async Task<bool> IsSpaceAvailableAsync(string vehicleType)
        {
            return await _context.ParkingSpaces
                .AnyAsync(s => s.IsAvailable && s.VehicleType == vehicleType);
        }

        public async Task<bool> CheckOutVehicleAsync(Vehicle vehicle)
        {
            var parkingSpace = await _context.ParkingSpaces
                .FirstOrDefaultAsync(s => s.Id == vehicle.ParkingSpaceId);

            if (parkingSpace != null && !parkingSpace.IsAvailable)
            {
                parkingSpace.IsAvailable = true;
                vehicle.ExitTime = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
