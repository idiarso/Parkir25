using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Models
{
    public class Scanner
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public string? Location { get; set; }
        public string? IPAddress { get; set; }
        public string? SerialPort { get; set; }
        public int? BaudRate { get; set; }
        public string? ScannerModel { get; set; }
        public string? ConnectionType { get; set; }
        public string? ScanMode { get; set; }
        public string? ScanFormat { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? LastMaintenanceBy { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
    }
} 