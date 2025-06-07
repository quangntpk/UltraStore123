using System.Text.Json;
using System.Text.Json.Serialization;

namespace UltraStrore.Common
{
    public class ByteArrayConverter : JsonConverter<byte[]?>
    {
        public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;
            string? base64 = reader.GetString();
            return string.IsNullOrEmpty(base64) ? null : Convert.FromBase64String(base64);
        }

        public override void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(Convert.ToBase64String(value));
        }
    }
}