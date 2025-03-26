using System;
using System.Threading.Tasks;
using ParkIRC.Shared.Models;
using ParkIRC.Shared.Interfaces;

namespace ParkIRC.Data.Services
{
    public class TicketService : ITicketService
    {
        private readonly IPrinterService _printerService;

        public TicketService(IPrinterService printerService)
        {
            _printerService = printerService;
        }

        public async Task<string> GenerateTicketAsync(Vehicle vehicle)
        {
            // Generate ticket data
            string ticketData = $"\n" +
                $"Vehicle Number: {vehicle.PlateNumber}\n" +
                $"Entry Time: {vehicle.EntryTime}\n" +
                $"Vehicle Type: {vehicle.VehicleType}\n";

            // Print the ticket
            await _printerService.PrintTicketAsync(ticketData);
            return "Ticket generated successfully";
        }

        public string GenerateTicket(Vehicle vehicle)
        {
            // Generate ticket data
            string ticketData = $"\n" +
                $"Vehicle Number: {vehicle.PlateNumber}\n" +
                $"Entry Time: {vehicle.EntryTime}\n" +
                $"Vehicle Type: {vehicle.VehicleType}\n";

            // Print the ticket
            _printerService.PrintTicket(ticketData);
            return "Ticket generated successfully";
        }

        public async Task GenerateEntryTicketAsync(Vehicle vehicle)
        {
            string ticketNumber = Guid.NewGuid().ToString();
            await _printerService.PrintEntryTicket(ticketNumber, vehicle.PlateNumber, vehicle.EntryTime, vehicle.VehicleType);
        }

        public async Task GenerateExitTicketAsync(Vehicle vehicle)
        {
            if (vehicle.ExitTime == null)
            {
                throw new InvalidOperationException("Vehicle exit time is not set");
            }
            
            string ticketNumber = Guid.NewGuid().ToString();
            await _printerService.PrintExitTicket(ticketNumber, vehicle.PlateNumber, vehicle.EntryTime, vehicle.ExitTime.Value, vehicle.Fee ?? 0);
        }

        public void GenerateEntryTicket(Vehicle vehicle)
        {
            string ticketNumber = Guid.NewGuid().ToString();
            _printerService.PrintEntryTicket(ticketNumber, vehicle.PlateNumber, vehicle.EntryTime, vehicle.VehicleType);
        }

        public void GenerateExitTicket(Vehicle vehicle)
        {
            if (vehicle.ExitTime == null)
            {
                throw new InvalidOperationException("Vehicle exit time is not set");
            }
            
            string ticketNumber = Guid.NewGuid().ToString();
            _printerService.PrintExitTicket(ticketNumber, vehicle.PlateNumber, vehicle.EntryTime, vehicle.ExitTime.Value, vehicle.Fee ?? 0);
        }
    }
}
