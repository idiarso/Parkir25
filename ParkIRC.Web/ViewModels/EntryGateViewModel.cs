using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ParkIRC.Models;

namespace ParkIRC.Web.ViewModels
{
    public class EntryGateViewModel
    {
        [Display(Name = "Gate ID")]
        public string GateId { get; set; } = "GATE-01";

        [Display(Name = "Operator")]
        public string OperatorName { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Ready";

        [Display(Name = "Camera Active")]
        public bool IsCameraActive { get; set; } = false;

        [Display(Name = "Printer Active")]
        public bool IsPrinterActive { get; set; } = true;

        [Display(Name = "Offline Mode")]
        public bool IsOfflineMode { get; set; } = false;

        [Display(Name = "Last Sync")]
        public DateTime LastSync { get; set; } = DateTime.Now;

        public List<VehicleEntry> RecentEntries { get; set; } = new List<VehicleEntry>();
    }
} 