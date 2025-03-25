using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkIRC.Models
{
    public class MaintenanceLog
    {
        public int Id { get; set; }

        [Required]
        public string OperatorId { get; set; }

        [Required]
        [StringLength(50)]
        public string DeviceType { get; set; }

        [Required]
        public int DeviceId { get; set; }

        [Required]
        [StringLength(50)]
        public string MaintenanceType { get; set; }

        [Required]
        public DateTime MaintenanceDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public string? Description { get; set; }
        public string? Actions { get; set; }
        public string? Parts { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser Operator { get; set; }
    }
} 