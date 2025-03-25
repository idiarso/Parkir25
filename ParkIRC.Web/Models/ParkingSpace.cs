using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Models
{
    public class ParkingSpace
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Nama Area")]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Tipe Kendaraan")]
        public string Type { get; set; }

        [Display(Name = "Zona")]
        public string? Zone { get; set; }

        [Display(Name = "Lantai")]
        public int? Floor { get; set; }

        [Display(Name = "Nomor")]
        public int? Number { get; set; }

        [Display(Name = "Nomor Slot")]
        public string SpaceNumber { get; set; }

        [Display(Name = "Tipe Slot")]
        public string SpaceType { get; set; }

        [Display(Name = "Tarif per Jam")]
        [DataType(DataType.Currency)]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Terakhir Digunakan")]
        public DateTime? LastOccupiedTime { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Terisi")]
        public bool IsOccupied { get; set; }

        [Display(Name = "Catatan")]
        public string? Notes { get; set; }

        [Display(Name = "Tanggal Dibuat")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Terakhir Diperbarui")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Vehicle? CurrentVehicle { get; set; }
        public int? CurrentVehicleId { get; set; }
        
        public virtual ICollection<ParkingTransaction>? ParkingTransactions { get; set; }
        public virtual ICollection<VehicleEntry>? VehicleEntries { get; set; }
        public virtual ICollection<VehicleExit>? VehicleExits { get; set; }
    }
}