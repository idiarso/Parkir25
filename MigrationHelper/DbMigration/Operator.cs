using Microsoft.AspNetCore.Identity;

namespace DbMigration
{
    public class Operator : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? EmployeeId { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsOnDuty { get; set; }
        public DateTime? ShiftStartTime { get; set; }
        public DateTime? ShiftEndTime { get; set; }
        public string? AccessLevel { get; set; }
        public int? WorkstationId { get; set; }
        public bool IsActive { get; set; } = true;
        
        public ICollection<Shift>? Shifts { get; set; }
    }
    
    public class Shift
    {
        public int Id { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; }
        public DateTime? LastModified { get; set; }
        public string? ModifiedBy { get; set; }
        
        public ICollection<Operator>? Operators { get; set; }
        public ICollection<Vehicle>? Vehicles { get; set; }
    }
    
    public class Vehicle
    {
        public int Id { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public bool IsParked { get; set; }
        public string? ImagePath { get; set; }
        public int? ParkingSpaceId { get; set; }
        public int? ShiftId { get; set; }
        
        public ParkingSpace? ParkingSpace { get; set; }
        public Shift? Shift { get; set; }
        public ICollection<ParkingTransaction>? Transactions { get; set; }
    }
    
    public class ParkingSpace
    {
        public int Id { get; set; }
        public string SpaceNumber { get; set; } = string.Empty;
        public string SpaceType { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
        public decimal HourlyRate { get; set; }
        public int? CurrentVehicleId { get; set; }
        public string? Location { get; set; }
        public bool IsReserved { get; set; }
        public string? ReservedFor { get; set; }
        public DateTime? LastOccupiedTime { get; set; }
        
        public Vehicle? CurrentVehicle { get; set; }
        public ICollection<ParkingTransaction>? Transactions { get; set; }
    }
    
    public class ParkingTransaction
    {
        public int Id { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public int ParkingSpaceId { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal Amount { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? PaymentTime { get; set; }
        public string? OperatorId { get; set; }
        public string? Notes { get; set; }
        
        public Vehicle Vehicle { get; set; } = null!;
        public ParkingSpace ParkingSpace { get; set; } = null!;
    }
    
    public class ParkingTicket
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string BarcodeData { get; set; } = string.Empty;
        public DateTime IssueTime { get; set; }
        public int VehicleId { get; set; }
        public string? OperatorId { get; set; }
        public int? ShiftId { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedTime { get; set; }
        public string? ExitOperatorId { get; set; }
        
        public Vehicle Vehicle { get; set; } = null!;
        public Operator? IssuedByOperator { get; set; }
        public Shift? Shift { get; set; }
    }
    
    public class Journal
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string OperatorId { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        
        public Operator Operator { get; set; } = null!;
    }
    
    public class ParkingRateConfiguration
    {
        public int Id { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public decimal BaseRate { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal DailyRate { get; set; }
        public decimal WeeklyRate { get; set; }
        public decimal MonthlyRate { get; set; }
        public decimal PenaltyRate { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string LastModifiedBy { get; set; } = string.Empty;
        public DateTime LastModifiedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
    }
    
    public class CameraSettings
    {
        public int Id { get; set; }
        public string ProfileName { get; set; } = string.Empty;
        public int CameraIndex { get; set; }
        public int Resolution { get; set; }
        public int Brightness { get; set; }
        public int Contrast { get; set; }
        public int Saturation { get; set; }
        public int Exposure { get; set; }
        public string? LightingCondition { get; set; }
        public bool EnableTextRecognition { get; set; }
        public bool IsDefaultProfile { get; set; }
        public string? Notes { get; set; }
    }
    
    public class EntryGate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public int? CameraIndex { get; set; }
        public string? SerialPort { get; set; }
        public int? BaudRate { get; set; }
        public string? IpAddress { get; set; }
        public int? Port { get; set; }
        public string? GateType { get; set; }
        public bool IsActive { get; set; }
    }
    
    public class SiteSettings
    {
        public int Id { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? LogoPath { get; set; }
        public string? ThemeColor { get; set; }
        public bool? ShowLogo { get; set; }
        public DateTime LastUpdated { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
    }
    
    public class PrinterConfig
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Port { get; set; } = string.Empty;
        public string? ConnectionType { get; set; }
        public int? CharactersPerLine { get; set; }
        public bool? AutoCut { get; set; }
        public string? Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastChecked { get; set; }
    }
} 