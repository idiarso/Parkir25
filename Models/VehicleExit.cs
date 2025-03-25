using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkIRC.Models
{
    public class VehicleExit
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime ExitTime { get; set; }
        public string? ExitPhotoPath { get; set; }
        public string? Notes { get; set; }
        
        public decimal Fee { get; set; }
        public bool IsPaid { get; set; }
        
        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }
        
        public string? OperatorId { get; set; }
        [ForeignKey("OperatorId")]
        public virtual ApplicationUser? Operator { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 