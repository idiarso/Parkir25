using System;
using System.Threading.Tasks;

namespace ParkIRC.Shared.Interfaces
{
    public interface IPrinterService
    {
        Task PrintTicketAsync(string content);
        void PrintTicket(string content);
        Task PrintEntryTicket(string ticketNumber, string plateNumber, DateTime entryTime, string vehicleType);
        Task PrintExitTicket(string ticketNumber, string plateNumber, DateTime entryTime, DateTime exitTime, decimal fee);
    }
}
