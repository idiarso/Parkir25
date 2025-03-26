using Microsoft.Extensions.Configuration;

namespace ParkIRC.Extensions
{
    public static class ConfigurationExtensions
    {
        public static T GetValue<T>(this IConfiguration configuration, string key, T defaultValue = default)
        {
            try
            {
                var value = configuration.GetValue<T>(key);
                return value != null ? value : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static string? GetConnectionString(this IConfiguration configuration, string name, string? defaultValue = null)
        {
            try
            {
                var value = configuration.GetConnectionString(name);
                return value ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static T GetSection<T>(this IConfiguration configuration, string sectionName) where T : new()
        {
            try
            {
                var section = configuration.GetSection(sectionName);
                var value = section.Get<T>();
                return value != null ? value : new T();
            }
            catch
            {
                return new T();
            }
        }

        public static bool HasSection(this IConfiguration configuration, string sectionName)
        {
            try
            {
                var section = configuration.GetSection(sectionName);
                return section != null && section.Exists();
            }
            catch
            {
                return false;
            }
        }

        public static IEnumerable<T> GetArray<T>(this IConfiguration configuration, string key)
        {
            try
            {
                var section = configuration.GetSection(key);
                return section != null ? section.Get<T[]>() ?? Array.Empty<T>() : Array.Empty<T>();
            }
            catch
            {
                return Array.Empty<T>();
            }
        }

        public static IDictionary<string, string> GetKeyValuePairs(this IConfiguration configuration, string sectionName)
        {
            try
            {
                var section = configuration.GetSection(sectionName);
                return section != null ? section.GetChildren()
                    .ToDictionary(x => x.Key, x => x.Value ?? string.Empty) : new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        public static bool TryGetValue<T>(this IConfiguration configuration, string key, out T value)
        {
            try
            {
                value = configuration.GetValue<T>(key);
                return value != null;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        public static bool TryGetConnectionString(this IConfiguration configuration, string name, out string connectionString)
        {
            try
            {
                connectionString = configuration.GetConnectionString(name);
                return connectionString != null;
            }
            catch
            {
                connectionString = null;
                return false;
            }
        }

        public static bool TryGetSection<T>(this IConfiguration configuration, string sectionName, out T value) where T : new()
        {
            try
            {
                var section = configuration.GetSection(sectionName);
                value = section != null ? section.Get<T>() : new T();
                return value != null;
            }
            catch
            {
                value = new T();
                return false;
            }
        }

        public static string GetRequiredValue(this IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException($"Required configuration value '{key}' is missing.");
            return value;
        }

        public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
        {
            var connectionString = configuration.GetConnectionString(name);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Required connection string '{name}' is missing.");
            return connectionString;
        }

        public static T GetRequiredSection<T>(this IConfiguration configuration, string sectionName) where T : new()
        {
            var section = configuration.GetSection(sectionName);
            var value = section != null ? section.Get<T>() : new T();
            if (value == null)
                throw new InvalidOperationException($"Required configuration section '{sectionName}' is missing.");
            return value;
        }

        public static IConfiguration GetEnvironmentConfiguration(this IConfiguration configuration, string environmentName)
        {
            return configuration.GetSection($"Environments:{environmentName}");
        }

        public static bool IsEnvironment(this IConfiguration configuration, string environmentName)
        {
            return configuration["Environment"]?.Equals(environmentName, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public static bool IsDevelopment(this IConfiguration configuration)
        {
            return configuration.IsEnvironment("Development");
        }

        public static bool IsStaging(this IConfiguration configuration)
        {
            return configuration.IsEnvironment("Staging");
        }

        public static bool IsProduction(this IConfiguration configuration)
        {
            return configuration.IsEnvironment("Production");
        }

        public static IConfiguration Reload(this IConfiguration configuration)
        {
            if (configuration is IConfigurationRoot configRoot)
            {
                configRoot.Reload();
            }
            return configuration;
        }
    }
}