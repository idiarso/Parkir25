using System.ComponentModel.DataAnnotations;

namespace ParkIRC.ViewModels
{
    public class EditOperatorViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare("Password")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        [Required]
        [StringLength(50)]
        public string Position { get; set; }

        public bool IsOnDuty { get; set; }

        public string? DeviceInfo { get; set; }
    }
} 