using SkiaSharp;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sdcb.WordClouds;

public class SKColorJsonConverter : JsonConverter<SKColor>
{
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

    public override void Write(Utf8JsonWriter writer, SKColor value, JsonSerializerOptions options)
    {
        var cssColor = SKColorToCSSColor(value);
        writer.WriteStringValue(cssColor);
    }

    public static string SKColorToCSSColor(SKColor color)
    {
        return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}{(color.Alpha == 255 ? "" : $"{color.Alpha:X2}")}";
    }
}