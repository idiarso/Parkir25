using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Web.ViewModels
{
    public class BackupViewModel
    {
        [Required(ErrorMessage = "Nama backup wajib diisi")]
        public string Name { get; set; }
        
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? FilePath { get; set; }
        public long FileSize { get; set; }
        
        // Properties needed by ManagementController
        public string BackupName { get; set; } = string.Empty;
        public List<string> BackupOptions { get; set; } = new List<string> { "Database", "Uploads", "Config", "Logs" };
        public List<string> SelectedOptions { get; set; } = new List<string> { "Database" };
        public List<BackupInfo> AvailableBackups { get; set; } = new List<BackupInfo>();
        
        public BackupViewModel()
        {
            Name = string.Empty;
            CreatedAt = DateTime.Now;
        }

        public string BackupPath { get; set; } = string.Empty;
        public bool AutoBackup { get; set; }
        public string BackupSchedule { get; set; } = string.Empty;
        public int RetentionDays { get; set; }
        public DateTime LastBackup { get; set; }
        public string LastBackupSize { get; set; } = string.Empty;
        public string LastBackupStatus { get; set; } = string.Empty;
        public List<BackupFile> BackupFiles { get; set; } = new();
    }
    
    public class RestoreViewModel
    {
        [Required(ErrorMessage = "Pilih file backup untuk dipulihkan")]
        public string SelectedBackup { get; set; } = string.Empty;
        
        public bool RestoreDatabase { get; set; } = true;
        public bool RestoreUploads { get; set; } = false;
        public bool RestoreConfig { get; set; } = false;
        public List<BackupInfo> AvailableBackups { get; set; } = new List<BackupInfo>();
    }
    
    public class BackupInfo
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Size { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public bool HasDatabase { get; set; } = false;
        public bool HasUploads { get; set; } = false;
        public bool HasConfig { get; set; } = false;
        public bool HasLogs { get; set; } = false;
    }

    public class BackupFile
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
} 