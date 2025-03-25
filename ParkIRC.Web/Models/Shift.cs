using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using ParkIRC.Extensions;

namespace ParkIRC.Models
{
    public class Shift
    {
        private List<string>? _workDays;
        private string _workDaysString = string.Empty;

        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Nama Shift")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Waktu Mulai")]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        [Display(Name = "Waktu Selesai")]
        public TimeSpan EndTime { get; set; }
        
        [Display(Name = "Deskripsi")]
        public string? Description { get; set; }
        
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
        
        public string? OperatorId { get; set; }
        [ForeignKey("OperatorId")]
        public virtual ApplicationUser? Operator { get; set; }
        
        public virtual ICollection<Vehicle>? Vehicles { get; set; } = new List<Vehicle>();
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }
        
        [Display(Name = "Operators")]
        public ICollection<Operator> Operators { get; set; } = new List<Operator>();
        
        public Shift()
        {
            Name = string.Empty;
            Vehicles = new List<Vehicle>();
            IsActive = true;
        }

        [Required(ErrorMessage = "Nama shift wajib diisi")]
        public string ShiftName { get; set; }
        
        [Required(ErrorMessage = "Tanggal wajib diisi")]
        public DateTime Date { get; set; }
        
        [Required(ErrorMessage = "Jumlah operator maksimal wajib diisi")]
        [Range(1, 100, ErrorMessage = "Jumlah operator maksimal harus antara 1-100")]
        public int MaxOperators { get; set; }

        [NotMapped]
        public List<string> WorkDays
        {
            get
            {
                if (_workDays == null || (_workDays.Count == 0 && !string.IsNullOrEmpty(_workDaysString)))
                {
                    _workDays = new List<string>();
                    if (!string.IsNullOrEmpty(_workDaysString))
                    {
                        _workDays.AddRange(_workDaysString.Split(',', StringSplitOptions.RemoveEmptyEntries));
                    }
                }
                return _workDays;
            }
            set
            {
                _workDays = value ?? new List<string>();
                _workDaysString = string.Join(",", _workDays);
            }
        }

        [Column("WorkDays")]
        public string WorkDaysString
        {
            get => _workDaysString;
            set
            {
                _workDaysString = value ?? string.Empty;
                _workDays = null;
            }
        }

        // Helper method to check if a given time falls within this shift
        public bool IsTimeInShift(DateTime time)
        {
            var timeOfDay = time.TimeOfDay;
            // Using StartTime and EndTime directly as they are already TimeSpan objects
            
            if (EndTime > StartTime)
            {
                // Normal shift (e.g., 9:00 to 17:00)
                return timeOfDay >= StartTime && timeOfDay <= EndTime;
            }
            else
            {
                // Overnight shift (e.g., 22:00 to 6:00)
                return timeOfDay >= StartTime || timeOfDay <= EndTime;
            }
        }
    }
} 