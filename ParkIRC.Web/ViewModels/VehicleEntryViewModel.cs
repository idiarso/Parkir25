using System;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Web.ViewModels
{
    public class VehicleEntryViewModel
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

        [Display(Name = "Waktu Masuk")]
        public DateTime EntryTime { get; set; } = DateTime.Now;

        [Display(Name = "Tarif Per Jam")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Gerbang")]
        public string GateId { get; set; } = string.Empty;

        [Display(Name = "Operator")]
        public string OperatorId { get; set; } = string.Empty;

        [Display(Name = "Foto")]
        public string? ImagePath { get; set; }

        [Display(Name = "Catatan")]
        public string? Notes { get; set; }

        // Additional properties referenced in code
        public string VehicleNumber { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }
}
