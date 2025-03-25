using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Web.Models
{
    public class CustomValidationResult : ValidationResult
    {
        public CustomValidationResult(string errorMessage) : base(errorMessage)
        {
        }

        public CustomValidationResult(string errorMessage, IEnumerable<string> memberNames) 
            : base(errorMessage, memberNames)
        {
        }
    }
} 