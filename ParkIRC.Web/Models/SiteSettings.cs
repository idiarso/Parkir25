using System;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Web.Models
{
    public class SiteSettings
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Nama lokasi wajib diisi")]
        public string LocationName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Alamat wajib diisi")]
        public string Address { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Nomor telepon wajib diisi")]
        public string Phone { get; set; } = string.Empty;
        
        public string Logo { get; set; } = string.Empty;
        public string LogoPath { get; set; } = string.Empty;
        public string FaviconPath { get; set; } = string.Empty;
        public string FooterText { get; set; } = string.Empty;
        public string ThemeColor { get; set; } = "#007bff";
        public bool ShowLogo { get; set; } = true;

        public string SiteName { get; set; } = "ParkIRC";
        public string LogoUrl { get; set; } = string.Empty;
        public string Theme { get; set; } = "default";
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyPhone { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string CompanyWebsite { get; set; } = string.Empty;
        public string Currency { get; set; } = "IDR";
        public string CurrencySymbol { get; set; } = "Rp";
        public string CurrencyCode { get; set; } = "IDR";
        public string TimeZone { get; set; } = "Asia/Jakarta";
        public string DateFormat { get; set; } = "dd/MM/yyyy";
        public string TimeFormat { get; set; } = "HH:mm:ss";
        public bool RequireLogin { get; set; } = true;
        public bool EnableNotifications { get; set; } = true;
        public bool EnableEmailNotifications { get; set; } = false;
        public bool EnableSMSNotifications { get; set; } = false;
        public string SMTPServer { get; set; } = string.Empty;
        public int SMTPPort { get; set; } = 587;
        public string SMTPUsername { get; set; } = string.Empty;
        public string SMTPPassword { get; set; } = string.Empty;
        public string SMTPFrom { get; set; } = string.Empty;
        public bool SMTPUseSSL { get; set; }
        public string SMTPHost { get; set; } = string.Empty;
        public string SMSProvider { get; set; } = string.Empty;
        public string SMSApiKey { get; set; } = string.Empty;
        public string SMSFrom { get; set; } = string.Empty;
        public string BackupPath { get; set; } = string.Empty;
        public int BackupRetentionDays { get; set; } = 30;
        public bool AutoBackup { get; set; } = true;
        public bool EnableBackup { get; set; }
        public string BackupSchedule { get; set; } = "0 0 * * *"; // Daily at midnight
        public string ImageStoragePath { get; set; } = "wwwroot/images/vehicles";
        public int ImageRetentionDays { get; set; } = 30;
        public bool AutoDeleteImages { get; set; } = true;
        public string DefaultPrinter { get; set; } = string.Empty;
        public string DefaultCamera { get; set; } = string.Empty;
        public string DefaultScanner { get; set; } = string.Empty;
        public string DefaultGate { get; set; } = string.Empty;
        public string DefaultOperator { get; set; } = string.Empty;
        public string DefaultVehicleType { get; set; } = string.Empty;
        public string DefaultPaymentMethod { get; set; } = "Cash";
        public decimal DefaultHourlyRate { get; set; } = 5000;
        public decimal DefaultDailyRate { get; set; } = 50000;
        public decimal DefaultMonthlyRate { get; set; } = 500000;
        public int DefaultGracePeriodMinutes { get; set; } = 10;
        public bool EnableOvertime { get; set; } = true;
        public decimal OvertimeRate { get; set; } = 10000;
        public int OvertimeGracePeriodMinutes { get; set; } = 5;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public bool EnableDarkMode { get; set; } = false;
        public string UpdatedBy { get; set; } = string.Empty;
        public string SiteDescription { get; set; } = string.Empty;
        public bool EnableSMSReports { get; set; } = true;
        public string SMSReportSchedule { get; set; } = "daily";
        public string SMSReportTime { get; set; } = "00:00";
        public string SMSReportRecipients { get; set; } = string.Empty;
        public string SMSReportMessage { get; set; } = string.Empty;
        public bool EnableMaintenanceReports { get; set; } = true;
        public string MaintenanceReportSchedule { get; set; } = "weekly";
        public string MaintenanceReportTime { get; set; } = "00:00";
        public string MaintenanceReportRecipients { get; set; } = string.Empty;
        public string MaintenanceReportSubject { get; set; } = "Weekly Maintenance Report";
        public string MaintenanceReportBody { get; set; } = string.Empty;
        public bool EnableBackupReports { get; set; } = true;
        public string BackupReportSchedule { get; set; } = "daily";
        public string BackupReportTime { get; set; } = "00:00";
        public string BackupReportRecipients { get; set; } = string.Empty;
        public string BackupReportSubject { get; set; } = "Daily Backup Report";
        public string BackupReportBody { get; set; } = string.Empty;
        public bool EnableAuditReports { get; set; } = true;
        public string AuditReportSchedule { get; set; } = "weekly";
        public string AuditReportTime { get; set; } = "00:00";
        public string AuditReportRecipients { get; set; } = string.Empty;
        public string AuditReportSubject { get; set; } = "Weekly Audit Report";
        public string AuditReportBody { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public bool EnablePagination { get; set; } = true;
        public bool EnableSearch { get; set; } = true;
        public bool EnableSorting { get; set; } = true;
        public bool EnableFiltering { get; set; } = true;
        public bool EnableExport { get; set; } = true;
        public bool EnableImport { get; set; } = true;
        public bool EnablePrinting { get; set; } = true;
        public bool EnableEmailReports { get; set; } = true;
        public string EmailReportSchedule { get; set; } = "daily";
        public string EmailReportTime { get; set; } = "00:00";
        public string EmailReportRecipients { get; set; } = string.Empty;
        public string EmailReportSubject { get; set; } = "Daily Parking Report";
        public string EmailReportBody { get; set; } = string.Empty;
        public string PrinterName { get; set; } = string.Empty;
        public string PrinterPort { get; set; } = string.Empty;
        public int PrinterBaudRate { get; set; }
        public string CameraUrl { get; set; } = string.Empty;
        public string CameraUsername { get; set; } = string.Empty;
        public string CameraPassword { get; set; } = string.Empty;
        public string LicensePlateRecognitionUrl { get; set; } = string.Empty;
        public string LicensePlateRecognitionApiKey { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public int AuditLogRetentionDays { get; set; }
        public int DefaultPageSize { get; set; } = 10;
        public bool EnableMaintenanceMode { get; set; }
        public string MaintenanceMessage { get; set; } = string.Empty;
    }
} 