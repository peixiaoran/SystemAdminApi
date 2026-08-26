using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemAdmin.Model.ModelHelper.ModelConverter
{
    /// <summary>
    /// DateTime 类型仅保留日期部分（yyyy-MM-dd）的JsonConverter
    /// </summary>
    public class DateOnlyStringConverter : JsonConverter<DateTime>
    {
        private const string Format = "yyyy-MM-dd";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }
}
