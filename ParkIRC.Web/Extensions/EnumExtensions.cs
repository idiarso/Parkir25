using System.ComponentModel;
using System.Reflection;

namespace ParkIRC.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        public static T GetValueFromDescription<T>(this string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException($"No enum value with description '{description}' found in {typeof(T)}");
        }

        public static IEnumerable<T> GetValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static Dictionary<string, string> GetEnumDescriptions<T>() where T : Enum
        {
            return GetValues<T>().ToDictionary(
                e => e.ToString(),
                e => e.GetDescription()
            );
        }

        public static bool HasFlag(this Enum value, Enum flag)
        {
            if (value == null)
                return false;

            var valueType = value.GetType();
            if (flag.GetType() != valueType)
                return false;

            var underlyingType = Enum.GetUnderlyingType(valueType);
            var valueBytes = BitConverter.GetBytes(Convert.ToUInt64(value));
            var flagBytes = BitConverter.GetBytes(Convert.ToUInt64(flag));

            for (var i = 0; i < valueBytes.Length; i++)
            {
                if ((valueBytes[i] & flagBytes[i]) != flagBytes[i])
                    return false;
            }

            return true;
        }
    }
} 