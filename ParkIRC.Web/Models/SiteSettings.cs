using System;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Models
{
    public class SiteSettings
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Nama lokasi wajib diisi")]
        public string LocationName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Alamat wajib diisi")]
        public string Address { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Nomor telepon wajib diisi")]
        public string Phone { get; set; } = string.Empty;
        
        public string Logo { get; set; } = string.Empty;
        public string LogoPath { get; set; } = string.Empty;
        public string FaviconPath { get; set; } = string.Empty;
        public string FooterText { get; set; } = string.Empty;
        public string ThemeColor { get; set; } = "#007bff";
        public bool ShowLogo { get; set; } = true;

        public string SiteName { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string Theme { get; set; } = "light";
        public string CompanyName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public bool EnableDarkMode { get; set; } = false;
        public string TimeZone { get; set; } = "Asia/Jakarta";
        public string Currency { get; set; } = "IDR";
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
} 