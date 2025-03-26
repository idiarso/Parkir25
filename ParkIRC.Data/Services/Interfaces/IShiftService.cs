using System.Threading.Tasks;
using ParkIRC.Data.Models;

namespace ParkIRC.Data.Services.Interfaces
{
    public interface IShiftService
    {
        Task<Shift> StartShiftAsync(string operatorName);
        Task EndShiftAsync(int shiftId);
        Task<Shift> GetCurrentShiftAsync();
    }
}
