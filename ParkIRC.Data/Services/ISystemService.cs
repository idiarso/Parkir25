using System.Threading.Tasks;

namespace ParkIRC.Data.Services
{
    public interface ISystemService
    {
        Task<bool> IsSystemOnlineAsync();
        Task<string> GetSystemStatusAsync();
        Task SetSystemStatusAsync(string status);
    }
}
