using System;
using System.Collections.Generic;

namespace ParkIRC.Models.ViewModels
{
    public class VehicleHistoryPageViewModel
    {
        public List<ParkingTransaction> Transactions { get; set; } = new();
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string SortBy { get; set; } = "Date";
        public bool SortAscending { get; set; } = false;
        public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);
    }
} 