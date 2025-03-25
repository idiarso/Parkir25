using System;
using System.Collections.Generic;

namespace ParkIRC.Models.ViewModels
{
    public class BackupViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        
        // Properties needed by ManagementController
        public string BackupName { get; set; } = string.Empty;
        public List<string> BackupOptions { get; set; } = new List<string> { "Database", "Uploads", "Config", "Logs" };
        public List<string> SelectedOptions { get; set; } = new List<string> { "Database" };
        public List<string> AvailableBackups { get; set; } = new List<string>();
    }
} 