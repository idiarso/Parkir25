using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Models
{
    public class Printer
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
        public int? PaperWidth { get; set; }
        public int? PaperHeight { get; set; }
        public string? PrinterModel { get; set; }
        public string? ConnectionType { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? LastMaintenanceBy { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
    }
} 