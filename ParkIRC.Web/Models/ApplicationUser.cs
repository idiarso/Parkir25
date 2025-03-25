using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ParkIRC.Models
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
        
        [StringLength(255)]
        [Display(Name = "Alamat")]
        public string? Address { get; set; }
        
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Tanggal Bergabung")]
        public DateTime JoinDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Foto Profil")]
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
        
        public virtual ICollection<ParkingTransaction> Transactions { get; set; } = new List<ParkingTransaction>();
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? LastLogin { get; set; }
        
        public ApplicationUser()
        {
            IsActive = true;
            IsOperator = false;
        }
    }
} 