using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Models
{
    public class ConnectionStatus
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string DeviceType { get; set; }

        [Required]
        public int DeviceId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        public DateTime LastChecked { get; set; }

        public string? LastError { get; set; }
        public int RetryCount { get; set; }
        public DateTime? LastSuccessfulConnection { get; set; }
        public string? ConnectionDetails { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 