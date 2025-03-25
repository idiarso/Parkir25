using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkIRC.Models
{
    public class ExitModel
    {
        [Required]
        public string TicketNumber { get; set; } = string.Empty;

        [Required]
        public string VehicleNumber { get; set; } = string.Empty;

        [Required]
        public string PaymentMethod { get; set; } = "Cash";

        [Required]
        public DateTime ExitTime { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        public string? GateId { get; set; }

        public string? OperatorId { get; set; }

        public string? ImagePath { get; set; }

        public string? Notes { get; set; }
    }
} 