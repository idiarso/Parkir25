using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdminCreator.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        public DateTime? BirthDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        
        public string? ProfilePhotoPath { get; set; }
        
        public string FullName 
        { 
            get
            {
                return $"{FirstName} {LastName}".Trim();
            }
        }
        
        public bool IsOperator { get; set; }
        
        public string? Notes { get; set; }
        
        public virtual ICollection<object> Transactions { get; set; } = new List<object>();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? LastLogin { get; set; }

        public string? Position { get; set; }
        public string? EmployeeId { get; set; }
        public bool IsOnDuty { get; set; }
        public DateTime? ShiftStartTime { get; set; }
        public DateTime? ShiftEndTime { get; set; }
        public string? AccessLevel { get; set; }
        public int? WorkstationId { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public ApplicationUser()
        {
            IsActive = true;
            IsOperator = false;
            IsOnDuty = false;
        }
    }
} 