using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdminCreator.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        [Display(Name = "Nama Depan")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Nama Belakang")]
        public string LastName { get; set; } = string.Empty;
        
        [Display(Name = "Tanggal Lahir")]
        public DateTime? BirthDate { get; set; }
        
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Tanggal Bergabung")]
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Foto Profil")]
        public string? ProfilePhotoPath { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}".Trim();
            }
        }
        
        public bool IsOperator { get; set; }
        
        public string? Notes { get; set; }
        
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
        
        public ApplicationUser()
        {
            IsActive = true;
            IsOperator = false;
            IsOnDuty = false;
        }
    }
} 