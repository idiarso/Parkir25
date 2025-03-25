using System.Text.Json;
using System.Text.Json.Serialization;

namespace ParkIRC.Extensions
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions DefaultOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        public static string ToJson<T>(this T value, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Serialize(value, options ?? DefaultOptions);
        }

        public static T FromJson<T>(this string json, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
        }

        public static object FromJson(this string json, Type type, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Deserialize(json, type, options ?? DefaultOptions);
        }

        public static bool TryFromJson<T>(this string json, out T result, JsonSerializerOptions? options = null)
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public static bool TryToJson<T>(this T value, out string json, JsonSerializerOptions? options = null)
        {
            try
            {
                json = JsonSerializer.Serialize(value, options ?? DefaultOptions);
                return true;
            }
            catch
            {
                json = null;
                return false;
            }
        }

        public static T Clone<T>(this T value)
        {
            return value.ToJson().FromJson<T>();
        }

        public static IDictionary<string, object> ToJsonDictionary<T>(this T value)
        {
            return JsonSerializer.Deserialize<IDictionary<string, object>>(
                JsonSerializer.Serialize(value, DefaultOptions),
                DefaultOptions);
        }

        public static T ToObject<T>(this IDictionary<string, object> dictionary)
        {
            return JsonSerializer.Deserialize<T>(
                JsonSerializer.Serialize(dictionary, DefaultOptions),
                DefaultOptions);
        }

        public static bool IsValidJson(this string json)
        {
            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static JsonDocument ToJsonDocument(this string json)
        {
            return JsonDocument.Parse(json);
        }

        public static JsonElement GetElement(this JsonDocument document, string path)
        {
            var current = document.RootElement;
            var segments = path.Split('.');

            foreach (var segment in segments)
            {
                if (current.ValueKind != JsonValueKind.Object)
                    throw new InvalidOperationException($"Cannot access property '{segment}' on non-object value");

                if (!current.TryGetProperty(segment, out var next))
                    throw new InvalidOperationException($"Property '{segment}' not found");

                current = next;
            }

            return current;
        }

        public static T GetValue<T>(this JsonElement element)
        {
            return JsonSerializer.Deserialize<T>(element.GetRawText(), DefaultOptions);
        }

        public static bool TryGetValue<T>(this JsonElement element, out T value)
        {
            try
            {
                value = JsonSerializer.Deserialize<T>(element.GetRawText(), DefaultOptions);
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        public static string PrettifyJson(this string json)
        {
            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                return json;
            }
        }

        public static string MinifyJson(this string json)
        {
            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = false });
            }
            catch
            {
                return json;
            }
        }

        public static JsonSerializerOptions CreateOptions(
            bool propertyNameCaseInsensitive = true,
            JsonNamingPolicy propertyNamingPolicy = null,
            JsonIgnoreCondition defaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            bool writeIndented = true)
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = propertyNameCaseInsensitive,
                PropertyNamingPolicy = propertyNamingPolicy ?? JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = defaultIgnoreCondition,
                WriteIndented = writeIndented
            };
        }
    }
} 