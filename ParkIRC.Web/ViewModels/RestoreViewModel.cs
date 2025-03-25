using System.ComponentModel.DataAnnotations;

namespace ParkIRC.ViewModels
{
    public class RestoreViewModel
    {
        [Required]
        public string BackupFile { get; set; }

        public bool OverwriteExisting { get; set; }

        public bool RestoreImages { get; set; }

        public bool RestoreLogs { get; set; }
    }
} 