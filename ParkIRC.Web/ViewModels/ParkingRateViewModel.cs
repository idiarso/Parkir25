using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Models.ViewModels
{
    public class ParkingRateViewModel
    {
        [Required]
        [StringLength(50)]
        public string VehicleType { get; set; }

        [Required]
        public decimal BaseRate { get; set; }

        [Required]
        public int GracePeriodMinutes { get; set; }

        [Required]
        public decimal HourlyRate { get; set; }

        [Required]
        public decimal DailyRate { get; set; }

        public decimal? WeeklyRate { get; set; }

        public decimal? MonthlyRate { get; set; }

        public string? Description { get; set; }
        
        [Display(Name = "Tarif Motor")]
        public decimal MotorcycleRate { get; set; }
        
        [Display(Name = "Tarif Mobil")]
        public decimal CarRate { get; set; }
        
        [Display(Name = "Tarif Jam Tambahan")]
        public decimal AdditionalHourRate { get; set; }
        
        [Display(Name = "Tarif Maksimum per Hari")]
        public decimal MaximumDailyRate { get; set; }

        [Required]
        [Display(Name = "Tarif Truk per Jam")]
        [Range(0, double.MaxValue, ErrorMessage = "Tarif truk per jam tidak boleh negatif")]
        public decimal TruckHourlyRate { get; set; }

        [Required]
        [Display(Name = "Tarif Bus per Jam")]
        [Range(0, double.MaxValue, ErrorMessage = "Tarif bus per jam tidak boleh negatif")]
        public decimal BusHourlyRate { get; set; }

        [Required]
        [Display(Name = "Tarif Mobil per Jam")]
        [Range(0, double.MaxValue, ErrorMessage = "Tarif mobil per jam tidak boleh negatif")]
        public decimal CarHourlyRate { get; set; }

        [Required]
        [Display(Name = "Tarif Motor per Jam")]
        [Range(0, double.MaxValue, ErrorMessage = "Tarif motor per jam tidak boleh negatif")]
        public decimal MotorcycleHourlyRate { get; set; }
    }
} 