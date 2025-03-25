using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkIRC.Models
{
    public class VehicleEntry
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(20)]
        [Display(Name = "Nomor Plat")]
        public string PlateNumber { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Jenis Kendaraan")]
        public string VehicleType { get; set; }
        
        [Required]
        [Display(Name = "Waktu Masuk")]
        public DateTime EntryTime { get; set; }
        
        [Display(Name = "Foto Masuk")]
        public string? EntryPhotoPath { get; set; }
        
        public string? Notes { get; set; }
        
        public int VehicleId { get; set; }
        
        public int? ParkingSpaceId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string OperatorId { get; set; }
        
        // Navigation properties
        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }
        
        [ForeignKey("ParkingSpaceId")]
        public virtual ParkingSpace? ParkingSpace { get; set; }
        
        public VehicleEntry()
        {
            PlateNumber = string.Empty;
            VehicleType = "Mobil";
            EntryTime = DateTime.Now;
        }
    }
} 