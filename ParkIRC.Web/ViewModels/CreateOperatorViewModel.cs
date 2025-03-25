using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ParkIRC.Web.ViewModels
{
    public class CreateOperatorViewModel
    {
        [Required(ErrorMessage = "Nama operator wajib diisi")]
        [Display(Name = "Nama Operator")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username wajib diisi")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Konfirmasi password wajib diisi")]
        [Display(Name = "Konfirmasi Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password tidak cocok")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi")]
        [EmailAddress(ErrorMessage = "Format email tidak valid")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jabatan wajib diisi")]
        [Display(Name = "Jabatan")]
        public string Position { get; set; } = string.Empty;

        [Display(Name = "Level Akses")]
        public string Role { get; set; } = "Operator";

        [Display(Name = "Gerbang")]
        public string GateId { get; set; } = string.Empty;

        [Display(Name = "Catatan")]
        public string? Notes { get; set; }

        public List<SelectListItem> AvailableRoles { get; set; } = new();

        // Additional properties referenced in code
        public ParkIRC.Models.Operator Operator { get; set; } = new();
        public string SelectedRole { get; set; } = string.Empty;
    }
} 