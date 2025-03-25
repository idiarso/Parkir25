using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Models
{
    public class Settings
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Key { get; set; }
        
        [Required]
        public string Value { get; set; }
        
        public string? Description { get; set; }
        public string? Group { get; set; }
        public bool IsSystem { get; set; }
        public bool IsEncrypted { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public Settings()
        {
            Key = string.Empty;
            Value = string.Empty;
            CreatedAt = DateTime.Now;
        }
    }
} 