using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ParkIRC.Extensions
{
    public static class ValidationExtensions
    {
        public static IEnumerable<ValidationResult> ValidateObject(this object instance)
        {
            var validationContext = new ValidationContext(instance);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(instance, validationContext, validationResults, true);
            return validationResults;
        }

        public static bool IsValid(this object instance)
        {
            return !ValidateObject(instance).Any();
        }

        public static string GetValidationErrors(this object instance)
        {
            return string.Join(Environment.NewLine, ValidateObject(instance).Select(r => r.ErrorMessage));
        }

        public static IEnumerable<ValidationResult> ValidateProperty(this object instance, string propertyName)
        {
            var propertyInfo = instance.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException($"Property {propertyName} not found on type {instance.GetType().Name}");

            var validationContext = new ValidationContext(instance) { MemberName = propertyName };
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateProperty(propertyInfo.GetValue(instance), validationContext, validationResults);
            return validationResults;
        }

        public static bool IsValidProperty(this object instance, string propertyName)
        {
            return !ValidateProperty(instance, propertyName).Any();
        }

        public static string GetPropertyValidationErrors(this object instance, string propertyName)
        {
            return string.Join(Environment.NewLine, ValidateProperty(instance, propertyName).Select(r => r.ErrorMessage));
        }

        public static IEnumerable<ValidationAttribute> GetValidationAttributes<T>(this string propertyName)
        {
            var propertyInfo = typeof(T).GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException($"Property {propertyName} not found on type {typeof(T).Name}");

            return propertyInfo.GetCustomAttributes<ValidationAttribute>();
        }

        public static bool HasRequiredAttribute<T>(this string propertyName)
        {
            return GetValidationAttributes<T>(propertyName).Any(a => a is RequiredAttribute);
        }

        public static bool HasStringLengthAttribute<T>(this string propertyName)
        {
            return GetValidationAttributes<T>(propertyName).Any(a => a is StringLengthAttribute);
        }

        public static StringLengthAttribute GetStringLengthAttribute<T>(this string propertyName)
        {
            return GetValidationAttributes<T>(propertyName).OfType<StringLengthAttribute>().FirstOrDefault();
        }

        public static bool HasRangeAttribute<T>(this string propertyName)
        {
            return GetValidationAttributes<T>(propertyName).Any(a => a is RangeAttribute);
        }

        public static RangeAttribute GetRangeAttribute<T>(this string propertyName)
        {
            return GetValidationAttributes<T>(propertyName).OfType<RangeAttribute>().FirstOrDefault();
        }

        public static bool HasRegularExpressionAttribute<T>(this string propertyName)
        {
            return GetValidationAttributes<T>(propertyName).Any(a => a is RegularExpressionAttribute);
        }

        public static RegularExpressionAttribute GetRegularExpressionAttribute<T>(this string propertyName)
        {
            return GetValidationAttributes<T>(propertyName).OfType<RegularExpressionAttribute>().FirstOrDefault();
        }
    }
} 