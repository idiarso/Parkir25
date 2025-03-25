using System.ComponentModel.DataAnnotations;

namespace ParkIRC.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
} 