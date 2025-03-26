using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Data.Data;
using ParkIRC.Data.Models;

namespace ParkIRC.Data.Services
{
    public class StorageService : IStorageService
    {
        private readonly ApplicationDbContext _context;

        public StorageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            using (var file = File.Create(filePath))
            {
                await fileStream.CopyToAsync(file);
            }
            return filePath;
        }

        public async Task<Stream> GetFileAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.OpenRead(filePath);
            }
            return null;
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }

        public async Task StoreVehicleAsync(Vehicle vehicle)
        {
            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task<ParkingSpace> GetAvailableSpaceAsync(string vehicleType)
        {
            return await _context.ParkingSpaces
                .FirstOrDefaultAsync(s => s.IsAvailable && s.VehicleType == vehicleType);
        }

        public async Task<ParkingSpace> GetAvailableSpaceAsync(Vehicle vehicle)
        {
            return await _context.ParkingSpaces
                .FirstOrDefaultAsync(s => s.IsAvailable && s.VehicleType == vehicle.VehicleType);
        }
    }
}
