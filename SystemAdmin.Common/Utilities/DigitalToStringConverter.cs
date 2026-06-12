using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemAdmin.Model.ModelHelper.ModelConverter
{
    /// <summary>
    /// long / long? 类型转换为 string 类型的 JsonConverter
    /// </summary>
    public class LongToStringConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(long)
                || typeToConvert == typeof(long?);
        }

        public override JsonConverter CreateConverter(
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (typeToConvert == typeof(long))
            {
                return new LongConverter();
            }

            if (typeToConvert == typeof(long?))
            {
                return new NullableLongConverter();
            }

            throw new JsonException($"LongToStringConverter 不支持类型：{typeToConvert}");
        }

        private class LongConverter : JsonConverter<long>
        {
            public override long Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var stringValue = reader.GetString();

                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        throw new JsonException("long 类型不能为 null 或空字符串。");
                    }

                    return long.Parse(stringValue);
                }

                if (reader.TokenType == JsonTokenType.Number)
                {
                    return reader.GetInt64();
                }

                throw new JsonException("long 类型必须是字符串或数字。");
            }

            public override void Write(
                Utf8JsonWriter writer,
                long value,
                JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        private class NullableLongConverter : JsonConverter<long?>
        {
            public override long? Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return null;
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    var stringValue = reader.GetString();

                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        return null;
                    }

                    return long.Parse(stringValue);
                }

                if (reader.TokenType == JsonTokenType.Number)
                {
                    return reader.GetInt64();
                }

                throw new JsonException("long? 类型必须是字符串、数字或 null。");
            }

            public override void Write(
                Utf8JsonWriter writer,
                long? value,
                JsonSerializerOptions options)
            {
                if (value.HasValue)
                {
                    writer.WriteStringValue(value.Value.ToString());
                }
                else
                {
                    writer.WriteNullValue();
                }
            }
        }
    }

    /// <summary>
    /// int类型转换为string类型的JsonConverter
    /// </summary>
    public class IntToStringConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Expected string token type.");

            return reader.GetInt32();
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}