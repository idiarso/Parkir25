using System;

namespace ParkIRC.Data.Models.ViewModels
{
    public class VehicleEntryRequest
    {
        public string PlateNumber { get; set; }
        public string VehicleType { get; set; }
        public string EntryGate { get; set; }
        public string Operator { get; set; }
    }
}
