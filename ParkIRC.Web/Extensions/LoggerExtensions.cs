using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace ParkIRC.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogMethodEntry(this ILogger logger,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            logger.LogDebug("Entering method {MethodName} in {SourceFile} at line {LineNumber}",
                methodName, Path.GetFileName(sourceFilePath), sourceLineNumber);
        }

        public static void LogMethodExit(this ILogger logger,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            logger.LogDebug("Exiting method {MethodName} in {SourceFile} at line {LineNumber}",
                methodName, Path.GetFileName(sourceFilePath), sourceLineNumber);
        }

        public static void LogException(this ILogger logger,
            Exception exception,
            string? message = null,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            logger.LogError(exception,
                "{Message} in method {MethodName} in {SourceFile} at line {LineNumber}",
                message ?? exception.Message,
                methodName,
                Path.GetFileName(sourceFilePath),
                sourceLineNumber);
        }

        public static void LogPerformance(this ILogger logger,
            string operation,
            TimeSpan duration,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            logger.LogInformation(
                "Performance: {Operation} took {Duration}ms in method {MethodName} in {SourceFile} at line {LineNumber}",
                operation,
                duration.TotalMilliseconds,
                methodName,
                Path.GetFileName(sourceFilePath),
                sourceLineNumber);
        }

        public static IDisposable BeginScope(this ILogger logger,
            string operation,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return logger.BeginScope(new Dictionary<string, object>
            {
                ["Operation"] = operation,
                ["MethodName"] = methodName,
                ["SourceFile"] = Path.GetFileName(sourceFilePath),
                ["LineNumber"] = sourceLineNumber
            });
        }

        public static void LogSecurity(this ILogger logger,
            string action,
            string user,
            string resource,
            bool isSuccessful,
            string? details = null,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var level = isSuccessful ? LogLevel.Information : LogLevel.Warning;

            logger.Log(level,
                "Security: {Action} by user {User} on resource {Resource} was {Status}. {Details} in method {MethodName} in {SourceFile} at line {LineNumber}",
                action,
                user,
                resource,
                isSuccessful ? "successful" : "unsuccessful",
                details,
                methodName,
                Path.GetFileName(sourceFilePath),
                sourceLineNumber);
        }

        public static void LogAudit(this ILogger logger,
            string action,
            string user,
            string resource,
            string? oldValue = null,
            string? newValue = null,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            logger.LogInformation(
                "Audit: {Action} performed by user {User} on resource {Resource}. Old value: {OldValue}, New value: {NewValue} in method {MethodName} in {SourceFile} at line {LineNumber}",
                action,
                user,
                resource,
                oldValue,
                newValue,
                methodName,
                Path.GetFileName(sourceFilePath),
                sourceLineNumber);
        }

        public static void LogValidation(this ILogger logger,
            string entity,
            string property,
            string value,
            string error,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            logger.LogWarning(
                "Validation failed for {Entity}.{Property} with value {Value}: {Error} in method {MethodName} in {SourceFile} at line {LineNumber}",
                entity,
                property,
                value,
                error,
                methodName,
                Path.GetFileName(sourceFilePath),
                sourceLineNumber);
        }

        public static void LogDatabase(this ILogger logger,
            string operation,
            string table,
            string? details = null,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            logger.LogInformation(
                "Database: {Operation} on table {Table}. {Details} in method {MethodName} in {SourceFile} at line {LineNumber}",
                operation,
                table,
                details,
                methodName,
                Path.GetFileName(sourceFilePath),
                sourceLineNumber);
        }

        public static void LogApi(this ILogger logger,
            string method,
            string endpoint,
            int statusCode,
            TimeSpan duration,
            string? details = null,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var level = statusCode < 400 ? LogLevel.Information : LogLevel.Warning;

            logger.Log(level,
                "API: {Method} {Endpoint} returned {StatusCode} in {Duration}ms. {Details} in method {MethodName} in {SourceFile} at line {LineNumber}",
                method,
                endpoint,
                statusCode,
                duration.TotalMilliseconds,
                details,
                methodName,
                Path.GetFileName(sourceFilePath),
                sourceLineNumber);
        }

        public static IDisposable BeginPerformanceScope(this ILogger logger,
            string operation,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return new PerformanceScope(logger, operation, methodName, sourceFilePath, sourceLineNumber);
        }

        private class PerformanceScope : IDisposable
        {
            private readonly ILogger _logger;
            private readonly string _operation;
            private readonly string _methodName;
            private readonly string _sourceFile;
            private readonly int _lineNumber;
            private readonly System.Diagnostics.Stopwatch _stopwatch;

            public PerformanceScope(ILogger logger,
                string operation,
                string methodName,
                string sourceFile,
                int lineNumber)
            {
                _logger = logger;
                _operation = operation;
                _methodName = methodName;
                _sourceFile = sourceFile;
                _lineNumber = lineNumber;
                _stopwatch = System.Diagnostics.Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                _logger.LogPerformance(_operation, _stopwatch.Elapsed, _methodName, _sourceFile, _lineNumber);
            }
        }
    }
} 