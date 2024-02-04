using SkiaSharp;
using System.Text.Json;
using System;
using System.Text.Json.Serialization;

namespace Sdcb.WordClouds;

/// <summary>
/// Converts <see cref="SKPoint"/> to and from JSON using <see cref="Utf8JsonReader"/> and <see cref="Utf8JsonWriter"/>.
/// </summary>
public class SKPointJsonConverter : JsonConverter<SKPoint>
{
    /// <summary>
    /// Reads and converts the JSON to type <see cref="SKPoint"/>.
    /// </summary>
    /// <param name="reader">The reader to read JSON from.</param>
    /// <param name="typeToConvert">The type of object to convert to.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>The <see cref="SKPoint"/> value.</returns>
    /// <exception cref="JsonException">Thrown if JSON token pattern doesn't match the expected format of an array with two numbers.</exception>
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

    /// <summary>
    /// Writes the <see cref="SKPoint"/> value as JSON.
    /// </summary>
    /// <param name="writer">The writer to write JSON to.</param>
    /// <param name="value">The <see cref="SKPoint"/> value to write as JSON.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <remarks>
    /// The <see cref="SKPoint"/> is written as a JSON array with two numbers representing the X and Y coordinates.
    /// </remarks>
    public override void Write(Utf8JsonWriter writer, SKPoint value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteEndArray();
    }
}