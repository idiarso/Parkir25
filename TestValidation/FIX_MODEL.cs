using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Models
{
    public class ParkingRateConfiguration : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }
} 