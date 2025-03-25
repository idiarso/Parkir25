using System;
using System.Collections.Generic;
using ParkIRC.Models;

namespace ParkIRC.Web.ViewModels
{
    public class HistoryViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string SearchQuery { get; set; } = string.Empty;
        public List<TransactionHistoryItem> Transactions { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public int TotalTransactions { get; set; }
    }

    public class TransactionHistoryItem
    {
        public string Id { get; set; } = string.Empty;
        public string TransactionNumber { get; set; } = string.Empty;
        public string TicketNumber { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? PaymentTime { get; set; }
        public string EntryGate { get; set; } = string.Empty;
        public string ExitGate { get; set; } = string.Empty;
        public string EntryOperator { get; set; } = string.Empty;
        public string ExitOperator { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
    }
}
