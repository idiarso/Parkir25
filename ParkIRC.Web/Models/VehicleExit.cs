using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkIRC.Models
{
    public class VehicleExit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string PlateNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string VehicleType { get; set; }

        [Required]
        public DateTime ExitTime { get; set; }

        public string? ExitPhotoPath { get; set; }

        public int VehicleId { get; set; }

        public int? ParkingSpaceId { get; set; }

        [Required]
        [StringLength(100)]
        public string OperatorId { get; set; }

        public int TransactionId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Duration { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentStatus { get; set; }

        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }

        [ForeignKey("ParkingSpaceId")]
        public virtual ParkingSpace? ParkingSpace { get; set; }

        [ForeignKey("TransactionId")]
        public virtual ParkingTransaction? Transaction { get; set; }
    }
} 