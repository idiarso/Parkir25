using System;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Web.Models
{
    public class Camera
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Type { get; set; }

        [Required]
        public string? Status { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public string? Location { get; set; }
        public string? IPAddress { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? StreamUrl { get; set; }
        public string? SnapshotUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? LastMaintenanceBy { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
    }
} 