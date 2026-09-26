using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YoutubeDLSharp.Converters
{
    public class UnixTimestampConverter : JsonConverter<DateTime?>
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            double value = reader.TokenType == JsonTokenType.String
                ? Convert.ToDouble(reader.GetString(), CultureInfo.InvariantCulture)
                : reader.GetDouble();

            return Epoch.AddSeconds(value);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue((value.Value - Epoch).TotalSeconds);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

    // Newtonsoft's IsoDateTimeConverter had a configurable DateTimeFormat ("yyyyMMdd" here, matching
    // yt-dlp's upload_date/release_date/modified_date fields). System.Text.Json has no equivalent
    // built-in converter with a custom format string, so this is a small converter rather than a
    // one-line subclass like the original.
    public class CustomDateTimeConverter : JsonConverter<DateTime?>
    {
        private const string Format = "yyyyMMdd";

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
            {
                return parsed;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString(Format, CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
