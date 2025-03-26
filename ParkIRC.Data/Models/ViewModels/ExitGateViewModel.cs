using System;

namespace ParkIRC.Data.Models.ViewModels
{
    public class ExitGateViewModel
    {
        public string PlateNumber { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime ExitTime { get; set; }
        public decimal Fee { get; set; }
    }
}
