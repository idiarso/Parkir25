using System;
using System.Collections.Generic;

namespace ParkIRC.Web.ViewModels
{
    public class EntryGateMonitoringViewModel
    {
        public List<EntryGateStatusViewModel> EntryGates { get; set; } = new();
        public List<RecentTransactionViewModel> RecentTransactions { get; set; } = new();
    }

    public class EntryGateStatusViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public bool IsOpen { get; set; }
        public int VehiclesProcessed { get; set; }
        public DateTime? LastActivity { get; set; }
    }

    public class RecentTransactionViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string TicketNumber { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public string EntryPoint { get; set; } = string.Empty;
        public string OperatorId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
} 