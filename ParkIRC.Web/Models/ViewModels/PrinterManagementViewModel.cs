using System.Collections.Generic;

namespace ParkIRC.Models.ViewModels
{
    public class PrinterManagementViewModel
    {
        public List<string> AvailablePrinters { get; set; } = new List<string>();
        public string CurrentPrinter { get; set; } = string.Empty;
    }
} 