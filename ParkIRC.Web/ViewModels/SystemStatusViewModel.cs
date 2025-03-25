using System;

namespace ParkIRC.Web.ViewModels
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
        
        public string SystemStatus { get; set; } = "Unknown";
        public string DatabaseStatus { get; set; } = "Unknown";
        public string PostgresServiceStatus { get; set; } = "Running";
        public string WebServerStatus { get; set; } = "Running";
        public string CupsServiceStatus { get; set; } = "Running";
        
        public string DiskTotal { get; set; } = "0 GB";
        public string DiskUsed { get; set; } = "0 GB";
        public string DiskFree { get; set; } = "0 GB";
        public string DiskUsagePercent { get; set; } = "0%";
        
        public string MemoryTotal { get; set; } = "0 GB";
        public string MemoryUsed { get; set; } = "0 GB";
        public string MemoryFree { get; set; } = "0 GB";
        
        public string LoadAverage1 { get; set; } = "0.00";
        public string LoadAverage5 { get; set; } = "0.00";
        public string LoadAverage15 { get; set; } = "0.00";
        
        public DateTime LastChecked { get; set; } = DateTime.Now;
        public string ErrorMessage { get; set; } = string.Empty;
        
        public string Version { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public TimeSpan Uptime { get; set; }
        public string PrinterStatus { get; set; } = string.Empty;
        public string CameraStatus { get; set; } = string.Empty;
        public string NetworkStatus { get; set; } = string.Empty;
        public string LastBackup { get; set; } = string.Empty;
        public string DiskSpace { get; set; } = string.Empty;
        public string MemoryUsage { get; set; } = string.Empty;
        public string CpuUsage { get; set; } = string.Empty;
        public List<string> ActiveUsers { get; set; } = new();
        public List<string> RecentErrors { get; set; } = new();
        
        public string GetStatusClass(string status)
        {
            return status.ToLower() switch
            {
                "running" or "online" or "connected" or "ok" or "good" => "success",
                "warning" or "degraded" => "warning",
                "error" or "critical" or "stopped" or "offline" or "disconnected" => "danger",
                _ => "secondary"
            };
        }
    }
} 