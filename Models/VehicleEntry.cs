using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkIRC.Models
{
    public class VehicleEntry
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime EntryTime { get; set; }
        public string? EntryPhotoPath { get; set; }
        public string? Notes { get; set; }
        
        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }
        
        public int? ParkingSpaceId { get; set; }
        [ForeignKey("ParkingSpaceId")]
        public virtual ParkingSpace? ParkingSpace { get; set; }
        
        public string? OperatorId { get; set; }
        [ForeignKey("OperatorId")]
        public virtual ApplicationUser? Operator { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 