using System.ComponentModel.DataAnnotations;

namespace ParkIRC.ViewModels
{
    public class CreateOperatorViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(50)]
        public string Position { get; set; }

        public string? DeviceInfo { get; set; }
    }
} 