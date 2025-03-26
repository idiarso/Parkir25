using System.Threading.Tasks;
using ParkIRC.Shared.Models;

namespace ParkIRC.Shared.Interfaces
{
    public interface ITicketService
    {
        Task<string> GenerateTicketAsync(Vehicle vehicle);
        string GenerateTicket(Vehicle vehicle);
        Task GenerateEntryTicketAsync(Vehicle vehicle);
        Task GenerateExitTicketAsync(Vehicle vehicle);
        void GenerateEntryTicket(Vehicle vehicle);
        void GenerateExitTicket(Vehicle vehicle);
    }
}
