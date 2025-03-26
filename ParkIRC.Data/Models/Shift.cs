using System;

namespace ParkIRC.Data.Models
{
    public class Shift
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Operator { get; set; }
        public bool IsActive { get; set; }
    }
}
