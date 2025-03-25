using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Models
{
    public class SystemLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Level { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        [Required]
        public string Message { get; set; }

        public string? Source { get; set; }
        public string? Exception { get; set; }
        public string? StackTrace { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? IPAddress { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestMethod { get; set; }
        public string? RequestQuery { get; set; }
        public string? RequestBody { get; set; }
        public int? StatusCode { get; set; }
        public long? ElapsedMilliseconds { get; set; }

        public DateTime CreatedAt { get; set; }
    }
} 