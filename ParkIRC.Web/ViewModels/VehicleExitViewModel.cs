using System;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Web.ViewModels
{
    public class VehicleExitViewModel
    {
        [Required(ErrorMessage = "Nomor tiket wajib diisi")]
        [Display(Name = "Nomor Tiket")]
        public string TicketNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor plat wajib diisi")]
        [Display(Name = "Nomor Plat")]
        public string PlateNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Metode pembayaran wajib dipilih")]
        [Display(Name = "Metode Pembayaran")]
        public string PaymentMethod { get; set; } = "Cash";

        [Display(Name = "Waktu Keluar")]
        public DateTime ExitTime { get; set; } = DateTime.Now;

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
        public decimal PaymentAmount { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
    }
}
