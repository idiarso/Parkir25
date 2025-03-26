using System;

namespace ParkIRC.Data.Models.ViewModels
{
    public class EditOperatorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string Role { get; set; }
    }
}
