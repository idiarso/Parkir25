using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ParkIRC.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception exception)
        {
            var messages = new List<string>();
            var current = exception;

            while (current != null)
            {
                messages.Add(current.Message);
                current = current.InnerException;
            }

            return string.Join(" -> ", messages);
        }

        public static string GetFullStackTrace(this Exception exception)
        {
            var stackTrace = new StringBuilder();
            var current = exception;

            while (current != null)
            {
                if (!string.IsNullOrEmpty(current.StackTrace))
                {
                    if (stackTrace.Length > 0)
                        stackTrace.AppendLine("--- Inner Exception Stack Trace ---");
                    stackTrace.AppendLine(current.StackTrace);
                }
                current = current.InnerException;
            }

            return stackTrace.ToString();
        }

        public static IDictionary<string, string> GetExceptionData(this Exception exception)
        {
            var data = new Dictionary<string, string>();
            foreach (var key in exception.Data.Keys)
            {
                if (key != null)
                {
                    var value = exception.Data[key];
                    data[key.ToString()] = value?.ToString() ?? "null";
                }
            }
            return data;
        }

        public static string ToDetailedString(this Exception exception)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Exception Details:");
            sb.AppendLine($"Type: {exception.GetType().FullName}");
            sb.AppendLine($"Message: {exception.GetFullMessage()}");
            sb.AppendLine($"Source: {exception.Source}");
            sb.AppendLine($"Target Site: {exception.TargetSite}");

            if (exception.Data.Count > 0)
            {
                sb.AppendLine("Data:");
                foreach (var item in exception.GetExceptionData())
                {
                    sb.AppendLine($"  {item.Key}: {item.Value}");
                }
            }

            sb.AppendLine("Stack Trace:");
            sb.AppendLine(exception.GetFullStackTrace());

            return sb.ToString();
        }

        public static bool IsCritical(this Exception exception)
        {
            return exception is OutOfMemoryException
                || exception is StackOverflowException
                || exception is AccessViolationException
                || exception is AppDomainUnloadedException
                || exception is BadImageFormatException
                || exception is InvalidProgramException;
        }

        public static bool IsTransient(this Exception exception)
        {
            return exception is TimeoutException
                || exception is System.Net.Sockets.SocketException
                || exception is System.Net.Http.HttpRequestException
                || exception is System.IO.IOException
                || (exception.InnerException?.IsTransient() ?? false);
        }

        public static bool IsConcurrencyError(this Exception exception)
        {
            return exception is DbUpdateConcurrencyException
                || exception is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException;
        }

        public static bool IsConnectionError(this Exception exception)
        {
            return exception is System.Data.SqlClient.SqlException
                || exception is Microsoft.Data.SqlClient.SqlException
                || exception is System.Net.Sockets.SocketException;
        }

        public static void ThrowIfNull<T>(this T value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
        }

        public static void ThrowIfNullOrEmpty(this string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty.", paramName);
        }

        public static void ThrowIfNullOrWhiteSpace(this string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null, empty, or whitespace.", paramName);
        }

        public static void ThrowIfOutOfRange<T>(this T value, string paramName, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(paramName, $"Value must be between {min} and {max}.");
        }

        public static void ThrowIfNegative(this int value, string paramName)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(paramName, "Value cannot be negative.");
        }

        public static void ThrowIfNegativeOrZero(this int value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName, "Value must be positive.");
        }

        public static void ThrowIfNegative(this decimal value, string paramName)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(paramName, "Value cannot be negative.");
        }

        public static void ThrowIfNegativeOrZero(this decimal value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName, "Value must be positive.");
        }

        public static void ThrowIfEmpty<T>(this IEnumerable<T> collection, string paramName)
        {
            if (!collection.Any())
                throw new ArgumentException("Collection cannot be empty.", paramName);
        }

        public static void ThrowIfNotFound<T>(this T value, string paramName) where T : class
        {
            if (value == null)
                throw new KeyNotFoundException($"The requested {typeof(T).Name} was not found.");
        }

        public static void ThrowIfDuplicate<T>(this T value, string paramName, Func<T, bool> predicate)
        {
            if (predicate(value))
                throw new InvalidOperationException($"A {typeof(T).Name} with the same key already exists.");
        }
    }
} 