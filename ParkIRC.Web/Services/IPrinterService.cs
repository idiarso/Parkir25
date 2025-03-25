using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ParkIRC.Models;

namespace ParkIRC.Services
{
    public interface IPrinterService
    {
        Task<bool> PrintTicketAsync(string ticketNumber, string plateNumber, DateTime entryTime, string vehicleType);
        Task<bool> PrintEntryTicket(string ticketNumber, string plateNumber, DateTime entryTime, string vehicleType);
        Task<bool> PrintExitTicket(string ticketNumber, string plateNumber, DateTime entryTime, DateTime exitTime, decimal amount);
        Task<bool> PrintTicket(ParkingTicket ticket);
        Task<bool> PrintReceipt(ParkingTransaction transaction);
        Task<bool> TestPrint();
        Task<bool> IsPrinterReady();
        Task<string> GetPrinterStatus();
        bool CheckPrinterStatus();
        string GetDefaultPrinter();
        List<string> GetAvailablePrinters();
        Task<bool> PrintTicket(string printerId, TicketData ticketData);
        Dictionary<string, PrinterStatus> GetAllPrinterStatus();
    }
} 