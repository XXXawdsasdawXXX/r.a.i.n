using System;
using System.Globalization;
using Newtonsoft.Json;
using Unity.Mathematics;

namespace Core.Conversion
{

    public class Float2JsonConverter : JsonConverter<float2>
    {
        public override void WriteJson(JsonWriter writer, float2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WriteEndObject();
        }

        public override float2 ReadJson(JsonReader reader, Type objectType, float2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            float x = 0, y = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string prop = (string)reader.Value;
                    reader.Read();
                    
                    if (prop == "x")
                    {
                        x = float.Parse(reader.Value.ToString(), CultureInfo.InvariantCulture);
                    }

                    if (prop == "y")
                    {
                        y = float.Parse(reader.Value.ToString(), CultureInfo.InvariantCulture);
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
            }

            return new float2(x, y);
        }
    }
}