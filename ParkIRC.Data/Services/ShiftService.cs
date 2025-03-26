using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Data.Data;
using ParkIRC.Data.Models;
using ParkIRC.Data.Services.Interfaces;

namespace ParkIRC.Data.Services
{
    public class ShiftService : IShiftService
    {
        private readonly ApplicationDbContext _context;

        public ShiftService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Shift> StartShiftAsync(string operatorName)
        {
            var shift = new Shift
            {
                Name = $"Shift_{DateTime.Now:yyyyMMdd_HHmmss}",
                StartDateTime = DateTime.Now,
                Operator = operatorName,
                IsActive = true
            };

            await _context.Shifts.AddAsync(shift);
            await _context.SaveChangesAsync();
            return shift;
        }

        public async Task EndShiftAsync(int shiftId)
        {
            var shift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.Id == shiftId);

            if (shift != null && shift.EndDateTime == null)
            {
                shift.EndDateTime = DateTime.Now;
                shift.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Shift> GetCurrentShiftAsync()
        {
            return await _context.Shifts
                .FirstOrDefaultAsync(s => s.IsActive);
        }
    }
}
