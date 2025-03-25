using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkIRC.Web.Models
{
    public class VehicleExit
    {
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public string OperatorId { get; set; }

        [Required]
        public DateTime ExitTime { get; set; }

        [Required]
        public decimal Fee { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public string? Notes { get; set; }
        public string? ReceiptNumber { get; set; }
        public string? ImagePath { get; set; }

        // Navigation properties
        public virtual Vehicle Vehicle { get; set; }
        public virtual ApplicationUser Operator { get; set; }
    }
} 