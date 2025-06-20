using System;
using System.Globalization;
using Core.Data;
using Newtonsoft.Json;

namespace Core.Conversion
{
    public class ReactiveFloatJsonConverter : JsonConverter<ReactiveProperty<float>>
    {
        public override void WriteJson(JsonWriter writer, ReactiveProperty<float> value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Value);
        }

        public override ReactiveProperty<float> ReadJson(JsonReader reader, Type objectType,
            ReactiveProperty<float> existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
            {
                float parsedValue = Convert.ToSingle(reader.Value, CultureInfo.InvariantCulture);
                return new ReactiveProperty<float>(parsedValue);
            }

            if (reader.TokenType == JsonToken.String &&
                float.TryParse((string)reader.Value, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out float result))
            {
                return new ReactiveProperty<float>(result);
            }

            throw new JsonSerializationException(
                $"Unexpected token {reader.TokenType} when parsing ReactiveProperty<float>");
        }
    }
}