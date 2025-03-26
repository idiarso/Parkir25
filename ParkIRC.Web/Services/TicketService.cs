using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Models;
using ParkIRC.Data;
using QRCoder;
using System.IO;
using ParkIRC.Web.Data;
using ParkIRC.Web.Models;

namespace ParkIRC.Services
{
    public interface ITicketService
    {
        Task<ParkingTicket> GenerateTicketAsync(Vehicle vehicle, string operatorId);
        Task<bool> ValidateTicketAsync(string ticketNumber);
        Task<string> GenerateQRCodeAsync(string ticketNumber);
        Task<bool> PrintTicketAsync(string ticketNumber);
    }

    public class TicketService : ITicketService
    {
        private readonly ILogger<TicketService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IPrinterService _printerService;

        public TicketService(
            ILogger<TicketService> logger,
            ApplicationDbContext context,
            IPrinterService printerService)
        {
            _logger = logger;
            _context = context;
            _printerService = printerService;
        }

        public async Task<ParkingTicket> GenerateTicketAsync(Vehicle vehicle, string operatorId)
        {
            try
            {
                var ticketNumber = GenerateTicketNumber();
                var barcodeData = GenerateBarcodeData(vehicle);
                var qrCodeImage = await GenerateQRCodeAsync(ticketNumber);

                var ticket = new ParkingTicket
                {
                    TicketNumber = ticketNumber,
                    VehicleId = vehicle.Id,
                    Vehicle = vehicle,
                    EntryTime = DateTime.Now,
                    OperatorId = operatorId,
                    BarcodeData = barcodeData,
                    QRCodeImage = qrCodeImage,
                    IsValid = true
                };

                await _context.ParkingTickets.AddAsync(ticket);
                await _context.SaveChangesAsync();

                await _printerService.PrintTicketAsync(ticket.TicketNumber, vehicle.PlateNumber, DateTime.Now, vehicle.VehicleType);

                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate ticket");
                throw;
            }
        }

        public async Task<bool> ValidateTicketAsync(string ticketNumber)
        {
            try
            {
                var ticket = await _context.ParkingTickets
                    .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber);

                return ticket != null && ticket.IsValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate ticket");
                return false;
            }
        }

        public async Task<string> GenerateQRCodeAsync(string ticketNumber)
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(ticketNumber, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                var bytes = qrCode.GetGraphic(20);
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for ticket: {TicketNumber}", ticketNumber);
                return string.Empty;
            }
        }

        public async Task<bool> PrintTicketAsync(string ticketNumber)
        {
            try
            {
                var ticket = await _context.ParkingTickets
                    .Include(t => t.Vehicle)
                    .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber);

                if (ticket == null)
                {
                    _logger.LogWarning("Ticket not found: {TicketNumber}", ticketNumber);
                    return false;
                }

                var qrCode = await GenerateQRCodeAsync(ticket.TicketNumber);
                
                return await _printerService.PrintTicketAsync(
                    ticket.TicketNumber,
                    ticket.Vehicle?.VehicleNumber ?? "Unknown",
                    ticket.EntryTime,
                    ticket.Vehicle?.VehicleType ?? "Unknown");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing ticket: {TicketNumber}", ticketNumber);
                return false;
            }
        }

        private string GenerateTicketNumber()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999).ToString();
        }

        private string GenerateBarcodeData(Vehicle vehicle)
        {
            return $"{vehicle.VehicleNumber}|{vehicle.EntryTime:yyyyMMddHHmmss}";
        }
    }
}