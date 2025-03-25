using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Models
{
    public class CustomValidationResult : ValidationResult
    {
        public new string ErrorMessage { get; set; }
        public new IEnumerable<string> MemberNames { get; set; }

        public CustomValidationResult(string errorMessage, IEnumerable<string> memberNames)
            : base(errorMessage, memberNames)
        {
            ErrorMessage = errorMessage;
            MemberNames = memberNames;
        }
    }
} 