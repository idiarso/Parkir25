using System;
using System.ComponentModel.DataAnnotations;
using ParkIRC.Models;

namespace ParkIRC.Web.ViewModels
{
    public class OperatorViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string GateId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsOnDuty { get; set; }
        public DateTime LastLogin { get; set; }
        public string? DeviceInfo { get; set; }
        public string? Notes { get; set; }

        // Properties expected by the controller
        public Operator Operator { get; set; }

        public OperatorViewModel()
        {
            Username = string.Empty;
            Email = string.Empty;
            Position = string.Empty;
            Role = "Staff";
            Operator = new Operator();
        }
    }
} 