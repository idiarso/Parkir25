using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkIRC.Models
{
    public class ParkingTransaction
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Transaction Number")]
        public string TransactionNumber { get; set; } = string.Empty;
        
        [Display(Name = "Ticket Number")]
        public string TicketNumber { get; set; } = string.Empty;

        [Display(Name = "Vehicle ID")]
        public int VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }
        
        [NotMapped]
        [Display(Name = "Vehicle Number")]
        public string VehicleNumber { 
            get => Vehicle?.VehicleNumber ?? string.Empty; 
            set { if (Vehicle != null) Vehicle.VehicleNumber = value; } 
        }
        
        [NotMapped]
        [Display(Name = "Vehicle Type")]
        public string VehicleType { 
            get => Vehicle?.VehicleType ?? string.Empty; 
            set { if (Vehicle != null) Vehicle.VehicleType = value; } 
        }

        [Display(Name = "Parking Space ID")]
        public int? ParkingSpaceId { get; set; }

        [ForeignKey("ParkingSpaceId")]
        public virtual ParkingSpace? ParkingSpace { get; set; }
        
        [Display(Name = "Entry Point")]
        public string EntryPoint { get; set; } = string.Empty;
        
        [Display(Name = "Exit Point")]
        public string ExitPoint { get; set; } = string.Empty;
        
        [Display(Name = "Manual Entry")]
        public bool IsManualEntry { get; set; }
        
        [Display(Name = "Vehicle Image")]
        public string ImagePath { get; set; } = string.Empty;
        
        [Display(Name = "Vehicle Image Path")]
        public string VehicleImagePath { get; set; } = string.Empty;
        
        [Display(Name = "Entry Photo Path")]
        public string EntryPhotoPath { get; set; } = string.Empty;
        
        [Display(Name = "Exit Photo Path")]
        public string ExitPhotoPath { get; set; } = string.Empty;
        
        [Display(Name = "Operator ID")]
        public string OperatorId { get; set; } = string.Empty;

        [Display(Name = "Entry Time")]
        public DateTime EntryTime { get; set; }

        [Display(Name = "Exit Time")]
        public DateTime? ExitTime { get; set; }

        [Display(Name = "Duration (hours)")]
        public decimal Duration { get; set; }

        [Display(Name = "Hourly Rate")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Display(Name = "Total Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        [Display(Name = "Payment Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaymentAmount { get; set; }
        
        [Display(Name = "Cost")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }
        
        [Display(Name = "Fee")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Fee { get; set; }

        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Pending";

        [Display(Name = "Is Paid")]
        public bool IsPaid { get; set; } = false;

        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Cash";

        [Display(Name = "Payment Time")]
        public DateTime? PaymentTime { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";
        
        [Display(Name = "Is Offline Entry")]
        public bool IsOfflineEntry { get; set; } = false;
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }
    }
}