using SkiaSharp;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sdcb.WordClouds;

/// <summary>
/// A custom JSON converter that can serialize and deserialize <see cref="SKColor"/> objects.
/// </summary>
public class SKColorJsonConverter : JsonConverter<SKColor>
{
    /// <summary>
    /// Reads and converts the JSON to an <see cref="SKColor"/> object.
    /// </summary>
    /// <param name="reader">The reader to read JSON from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>A new instance of <see cref="SKColor"/>.</returns>
    /// <exception cref="JsonException">Thrown when the color string is in an invalid format.</exception>
    public override SKColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? colorString = reader.GetString();
        // Assuming colorString is in the format #RRGGBB or #RRGGBBAA
        if (!string.IsNullOrEmpty(colorString) && (colorString!.Length == 7 || colorString.Length == 9) && colorString[0] == '#')
        {
            byte r = Convert.ToByte(colorString.Substring(1, 2), 16);
            byte g = Convert.ToByte(colorString.Substring(3, 2), 16);
            byte b = Convert.ToByte(colorString.Substring(5, 2), 16);
            byte a = (colorString.Length == 9) ? Convert.ToByte(colorString.Substring(7, 2), 16) : (byte)255;
            return new SKColor(r, g, b, a);
        }
        else
        {
            throw new JsonException("Invalid color format.");
        }
    }

    /// <summary>
    /// Writes an <see cref="SKColor"/> object to JSON as a CSS compatible color string.
    /// </summary>
    /// <param name="writer">The writer to write JSON to.</param>
    /// <param name="value">The <see cref="SKColor"/> value to write.</param>
    /// <param name="options">The serializer options to use.</param>
    public override void Write(Utf8JsonWriter writer, SKColor value, JsonSerializerOptions options)
    {
        var cssColor = SKColorToCSSColor(value);
        writer.WriteStringValue(cssColor);
    }

    /// <summary>
    /// Converts an <see cref="SKColor"/> object to a CSS formatted color string.
    /// </summary>
    /// <param name="color">The SKColor object to convert.</param>
    /// <returns>A CSS color formatted string representation of the <see cref="SKColor"/>.</returns>
    internal static string SKColorToCSSColor(SKColor color)
    {
        return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}{(color.Alpha == 255 ? "" : $"{color.Alpha:X2}")}";
    }
}
