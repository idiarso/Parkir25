using System;

namespace ParkIRC.Web.ViewModels
{
    public class ReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string Format { get; set; } = "PDF";
        public bool IncludeCharts { get; set; } = true;
        public string? EmailRecipients { get; set; }
    }

    public class ReportFilter
    {
        public string Type { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
