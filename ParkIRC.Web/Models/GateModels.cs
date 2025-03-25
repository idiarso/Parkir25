using System;

namespace ParkIRC.Models
{
    // API Request Models
    public class GateCommandRequest
    {
        public string Command { get; set; }
        public string Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class CameraCaptureRequest
    {
        public string Reason { get; set; }
        public CameraMetadata Metadata { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class CameraMetadata
    {
        public string TicketId { get; set; }
        public string LicensePlate { get; set; }
    }

    public class PrintRequest
    {
        public string Type { get; set; } // TICKET or RECEIPT
        public PrintData Data { get; set; }
    }

    public class PrintData
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string LicensePlate { get; set; }
        public decimal? AmountPaid { get; set; }
        public string Duration { get; set; }
        public string Operator { get; set; }
    }

    // API Response Models
    public class CommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Command { get; set; }
        public string GateId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class CameraCaptureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string GateId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public ImageInfo ImageInfo { get; set; }
    }

    public class ImageInfo
    {
        public string ExpectedPath { get; set; }
    }

    public class PrintResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string GateId { get; set; }
        public string PrintType { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class GateStatusResponse
    {
        public string GateId { get; set; }
        public GateStatus Status { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class GateStatus
    {
        public string Gate { get; set; }
        public string Sensor { get; set; }
        public DateTime? LastCameraTrigger { get; set; }
        public DateTime? LastPrintJob { get; set; }
    }
} 