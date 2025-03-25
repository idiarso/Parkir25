using System;

namespace ParkIRC.Models.ViewModels
{
    public class EntryGateViewModel
    {
        public string VehicleNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string ParkingSpace { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsCameraActive { get; set; }
        public string LastCapturedImage { get; set; } = string.Empty;
        public CameraSettings CameraConfig { get; set; } = new();
    }
} 