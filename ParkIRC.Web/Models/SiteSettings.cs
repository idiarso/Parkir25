using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Web.Models
{
    public class SiteSettings
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SiteName { get; set; } = "ParkIRC";

        [StringLength(200)]
        public string? LogoUrl { get; set; }

        public bool ShowLogo { get; set; } = true;

        [Required]
        [StringLength(10)]
        public string Theme { get; set; } = "light";

        [Required]
        [StringLength(5)]
        public string CurrencySymbol { get; set; } = "$";

        [Required]
        [StringLength(20)]
        public string TimeFormat { get; set; } = "HH:mm";

        [Required]
        [StringLength(20)]
        public string DateFormat { get; set; } = "yyyy-MM-dd";
    }
}
