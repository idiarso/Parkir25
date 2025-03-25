using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Models.ViewModels
{
    public class VehicleHistoryViewModel
    {
        public string VehicleId { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastEntry { get; set; }
        public DateTime? LastExit { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalVisits { get; set; }
        public List<VehicleHistoryItemViewModel> History { get; set; } = new();
        
        // Additional properties required by ManagementController
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Vehicle { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        private TimeSpan? _duration;
        public TimeSpan Duration
        {
            get => _duration ?? (ExitTime.HasValue ? ExitTime.Value - EntryTime : TimeSpan.Zero);
            set => _duration = value;
        }
        public decimal Amount { get; set; }
        public string ParkingSpace { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string EntryPhotoPath { get; set; } = string.Empty;
        public string ExitPhotoPath { get; set; } = string.Empty;
    }

    public class VehicleHistoryItemViewModel
    {
        public string TransactionNumber { get; set; } = string.Empty;
        public string TicketNumber { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? PaymentTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string OperatorId { get; set; } = string.Empty;
        public string EntryPoint { get; set; } = string.Empty;
        private TimeSpan? _duration;
        public TimeSpan Duration
        {
            get => _duration ?? (ExitTime.HasValue ? ExitTime.Value - EntryTime : TimeSpan.Zero);
            set => _duration = value;
        }
    }
}
