using System;

namespace ParkIRC.Models.ViewModels
{
    public class SystemStatusViewModel
    {
        public bool DatabaseStatus { get; set; }
        public bool PostgresServiceStatus { get; set; }
        public bool CupsServiceStatus { get; set; }
        
        public string DiskTotal { get; set; } = string.Empty;
        public string DiskUsed { get; set; } = string.Empty;
        public string DiskFree { get; set; } = string.Empty;
        public string DiskUsagePercent { get; set; } = string.Empty;
        
        public string MemoryTotal { get; set; } = string.Empty;
        public string MemoryUsed { get; set; } = string.Empty;
        public string MemoryFree { get; set; } = string.Empty;
        
        public string SystemUptime { get; set; } = string.Empty;
        
        public string LoadAverage1 { get; set; } = string.Empty;
        public string LoadAverage5 { get; set; } = string.Empty;
        public string LoadAverage15 { get; set; } = string.Empty;
        
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; } = DateTime.Now;
        
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