using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Web.Models
{
    public class ParkingRate
    {
        public int Id { get; set; }

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

        [Required]
        public decimal LostTicketFee { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
} 