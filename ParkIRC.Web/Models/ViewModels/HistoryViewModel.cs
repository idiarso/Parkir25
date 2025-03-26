using System;
using System.Collections.Generic;

namespace ParkIRC.Models.ViewModels
{
    public class TransactionHistoryItem
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; }
        public string VehicleNumber { get; set; }
        public string PlateNumber { get; set; }
        public string VehicleType { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public string Duration { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? PaymentTime { get; set; }
        public string EntryGate { get; set; }
        public string ExitGate { get; set; }
        public string EntryOperator { get; set; }
        public string ExitOperator { get; set; }
        public bool IsPaid { get; set; }
    }

    public class HistoryViewModel
    {
        public List<TransactionHistoryItem> Transactions { get; set; } = new List<TransactionHistoryItem>();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTransactions { get; set; }
    }
}
