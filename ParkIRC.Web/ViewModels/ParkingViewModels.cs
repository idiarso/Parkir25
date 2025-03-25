using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Web.ViewModels
{
    public class ParkingViewModel
    {
        [Required(ErrorMessage = "Nomor plat wajib diisi")]
        [Display(Name = "Nomor Plat")]
        public string PlateNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tipe kendaraan wajib dipilih")]
        [Display(Name = "Tipe Kendaraan")]
        public string VehicleType { get; set; } = string.Empty;

        [Display(Name = "Nama Pengemudi")]
        public string? DriverName { get; set; }

        [Display(Name = "Nomor Telepon")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Gerbang")]
        public string GateId { get; set; } = string.Empty;

        [Display(Name = "Operator")]
        public string OperatorId { get; set; } = string.Empty;

        [Display(Name = "Foto")]
        public string? ImagePath { get; set; }

        [Display(Name = "Catatan")]
        public string? Notes { get; set; }
    }

    public class ParkingSpaceViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
        public string? CurrentVehiclePlate { get; set; }
        public DateTime? OccupiedSince { get; set; }
        public string Status { get; set; } = "Available";
    }
} 