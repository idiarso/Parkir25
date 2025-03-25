using System.Collections.Generic;

namespace ParkIRC.Models.ViewModels
{
    public class RestoreViewModel
    {
        public List<string> AvailableBackups { get; set; } = new List<string>();
        public string SelectedBackup { get; set; } = string.Empty;
        public bool RestoreDatabase { get; set; } = true;
        public bool RestoreUploads { get; set; } = true;
        public bool RestoreConfig { get; set; } = true;
        public bool RestoreLogs { get; set; } = false;
        public bool OverwriteExisting { get; set; } = false;
    }
} 