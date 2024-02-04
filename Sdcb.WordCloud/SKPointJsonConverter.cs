using SkiaSharp;
using System.Text.Json;
using System;
using System.Text.Json.Serialization;

namespace Sdcb.WordClouds;

public class SKPointJsonConverter : JsonConverter<SKPoint>
{
    public override SKPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("JSON payload expected to start with a StartArray token.");
        }

        reader.Read(); // Move to first number
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("X coordinate as a first number expected.");
        }
        float x = reader.GetSingle();

        reader.Read(); // Move to second number
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Y coordinate as a second number expected.");
        }
        float y = reader.GetSingle();

        reader.Read(); // Move past the end of the array
        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("JSON payload expected to end with an EndArray token.");
        }

        return new SKPoint(x, y);
    }

    public override void Write(Utf8JsonWriter writer, SKPoint value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteEndArray();
    }
}