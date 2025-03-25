using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestValidation
{
    public class Model : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Test validation");
        }
    }
}
