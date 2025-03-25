using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkIRC.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Vehicle Number")]
        public string VehicleNumber { get; set; } = string.Empty;
        
        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; } = string.Empty;
        
        [NotMapped]
        public string PlateNumber { 
            get => VehicleNumber; 
            set => VehicleNumber = value; 
        }
        
        [NotMapped]
        public string Type { 
            get => VehicleType; 
            set => VehicleType = value; 
        }
        
        [Display(Name = "Driver Name")]
        public string DriverName { get; set; } = string.Empty;
        
        [Display(Name = "Contact Number")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [NotMapped]
        [Display(Name = "Contact Number (Alt)")]
        public string ContactNumber { 
            get => PhoneNumber; 
            set => PhoneNumber = value; 
        }
        
        [Display(Name = "Entry Time")]
        public DateTime? EntryTime { get; set; }
        
        [Display(Name = "Exit Time")]
        public DateTime? ExitTime { get; set; }
        
        [Display(Name = "Is Parked")]
        public bool IsParked { get; set; } = true;
        
        [Display(Name = "Entry Photo")]
        public string EntryPhotoPath { get; set; } = string.Empty;
        
        [Display(Name = "Exit Photo")]
        public string ExitPhotoPath { get; set; } = string.Empty;
        
        [Display(Name = "Barcode Image")]
        public string BarcodeImagePath { get; set; } = string.Empty;
        
        [Display(Name = "Parking Space")]
        public int? ParkingSpaceId { get; set; }
        
        [ForeignKey("ParkingSpaceId")]
        public virtual ParkingSpace? ParkingSpace { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Merk")]
        public string? Brand { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Model")]
        public string? Model { get; set; }
        
        [StringLength(50)]
        [Display(Name = "Warna")]
        public string? Color { get; set; }
        
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Blacklist")]
        public bool IsBlacklisted { get; set; }
        
        [Display(Name = "Tanggal Dibuat")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Display(Name = "Tanggal Diperbarui")]
        public DateTime? UpdatedAt { get; set; }
        
        public string? Notes { get; set; }
        
        public int? ShiftId { get; set; }
        [ForeignKey("ShiftId")]
        public virtual Shift? Shift { get; set; }
        
        public virtual ICollection<ParkingTransaction>? ParkingTransactions { get; set; } = new List<ParkingTransaction>();
        public virtual ICollection<VehicleEntry>? VehicleEntries { get; set; } = new List<VehicleEntry>();
        public virtual ICollection<VehicleExit>? VehicleExits { get; set; } = new List<VehicleExit>();
    }
}