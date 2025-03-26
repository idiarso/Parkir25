using System;

namespace ParkIRC.Data.Models
{
    public class CameraConfig
    {
        public string CameraId { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }
    }
}
