using System.Threading.Tasks;
using ParkIRC.Data.Models;

namespace ParkIRC.Data.Services
{
    public interface ITicketService
    {
        Task<string> GenerateTicketAsync(Vehicle vehicle);
        string GenerateTicket(Vehicle vehicle);
    }
}
