namespace ParkIRC.ViewModels
{
    public class SystemStatusViewModel
    {
        public bool DatabaseConnected { get; set; }
        public bool PrinterConnected { get; set; }
        public bool ScannerConnected { get; set; }
        public bool CameraConnected { get; set; }
        public bool GateConnected { get; set; }
        public string? LastBackupDate { get; set; }
        public string? DiskSpaceAvailable { get; set; }
        public string? SystemUptime { get; set; }
        public int ActiveSessions { get; set; }
        public int PendingTransactions { get; set; }
    }
} 