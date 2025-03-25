using System;
using System.Collections.Generic;

namespace ParkIRC.Models.ViewModels
{
    public class VehicleHistoryPageViewModel
    {
        public List<VehicleHistoryViewModel> Transactions { get; set; } = new List<VehicleHistoryViewModel>();
        public string Status { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "EntryTime";
        public string SortDirection { get; set; } = "desc";
    }
} 