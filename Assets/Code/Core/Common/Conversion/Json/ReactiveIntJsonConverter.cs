using System;
using System.Globalization;
using Core.Data;
using Newtonsoft.Json;

namespace Core.Conversion
{
    public class ReactiveIntJsonConverter : JsonConverter<ReactiveProperty<int>>
    {
        public override void WriteJson(JsonWriter writer, ReactiveProperty<int> value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Value);
        }

        public override ReactiveProperty<int> ReadJson(JsonReader reader, Type objectType,
            ReactiveProperty<int> existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
            {
                int parsedValue = Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture);
                return new ReactiveProperty<int>(parsedValue);
            }

            if (reader.TokenType == JsonToken.String &&
                Int32.TryParse((string)reader.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out int result))
            {
                return new ReactiveProperty<int>(result);
            }

            throw new JsonSerializationException(
                $"Unexpected token {reader.TokenType} when parsing ReactiveProperty<float>");
        }
    }
}