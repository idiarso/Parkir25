using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ParkIRC.Models;
using System.Collections.Generic;

namespace ParkIRC.Web.ViewModels
{
    public class EditOperatorViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama operator wajib diisi")]
        [Display(Name = "Nama Operator")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username wajib diisi")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Password Baru")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }

        [Display(Name = "Konfirmasi Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password tidak cocok")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Level Akses")]
        public string Role { get; set; } = "Operator";

        [Display(Name = "Gerbang")]
        public string GateId { get; set; } = string.Empty;

        [Display(Name = "Catatan")]
        public string? Notes { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Jabatan")]
        public string Position { get; set; } = string.Empty;

        [Display(Name = "Sedang Bertugas")]
        public bool IsOnDuty { get; set; }

        public string? DeviceInfo { get; set; }
        
        // Navigation properties
        public Operator Operator { get; set; } = new();
        public List<SelectListItem> AvailableRoles { get; set; } = new();
        public string SelectedRole { get; set; } = "Staff";
    }
} 