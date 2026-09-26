using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YoutubeDLSharp.Converters
{
    // yt-dlp's enum-shaped fields are snake_case strings (e.g. "premium_only") that don't match
    // the C# member names (PremiumOnly) even case-insensitively, because of the underscore -
    // System.Text.Json's built-in JsonStringEnumConverter can't bridge that on its own. This
    // replaces Newtonsoft's StringEnumConverter + [EnumMember] combination by reading the same
    // [EnumMember(Value = "...")] attributes directly via reflection.
    public class StringToEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // yt-dlp emits some of these fields (e.g. has_drm) as raw JSON booleans rather than
            // the string "True"/"False", alongside genuine string values like "maybe" - map both
            // token shapes onto the same [EnumMember(Value = "...")] lookup below.
            string value;
            if (reader.TokenType == JsonTokenType.String)
            {
                value = reader.GetString();
            }
            else if (reader.TokenType == JsonTokenType.True)
            {
                value = "True";
            }
            else if (reader.TokenType == JsonTokenType.False)
            {
                value = "False";
            }
            else
            {
                value = null;
            }
            if (value == null)
            {
                return default;
            }

            foreach (var field in typeToConvert.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var member = field.GetCustomAttribute<EnumMemberAttribute>();
                var candidate = member?.Value ?? field.Name;
                if (string.Equals(candidate, value, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)field.GetValue(null);
                }
            }

            // Fall back to a direct name match for enums that don't declare [EnumMember].
            return Enum.TryParse<T>(value, true, out var parsed) ? parsed : default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var field = typeToConvert(value);
            var member = field?.GetCustomAttribute<EnumMemberAttribute>();
            writer.WriteStringValue(member?.Value ?? value.ToString());
        }

        private static FieldInfo typeToConvert(T value) =>
            typeof(T).GetField(value.ToString());
    }

    public class StringToNullableIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.TryGetInt32(out var direct) ? direct : (int)reader.GetDouble();
            }

            var value = reader.GetString();
            if (value == null)
            {
                return null;
            }

            var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (value.Contains(decimalSeparator))
            {
                value = value.Split(Convert.ToChar(decimalSeparator))[0];
            }

            return int.TryParse(value, out var intValue) ? intValue : (int?)null;
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
