using System.ComponentModel.DataAnnotations;

namespace ParkIRC.ViewModels
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
    }
} 