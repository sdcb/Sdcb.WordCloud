using SkiaSharp;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sdcb.WordClouds;

public class SKTypefaceConverter : JsonConverter<SKTypeface>
{
    public override SKTypeface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string familyName = reader.GetString()!;
            return SKTypeface.FromFamilyName(familyName);
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            string? familyName = null;
            int weight = (int)SKFontStyleWeight.Normal;
            int width = (int)SKFontStyleWidth.Normal;
            SKFontStyleSlant slant = SKFontStyleSlant.Upright;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    SKFontStyle fontStyle = new(weight, width, slant);
                    return SKTypeface.FromFamilyName(familyName, fontStyle);
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString()!;
                    reader.Read();

                    switch (propertyName)
                    {
                        case "familyName":
                            familyName = reader.GetString();
                            break;
                        case "weight":
                            weight = reader.GetInt32();
                            break;
                        case "width":
                            width = reader.GetInt32();
                            break;
                        case "slant":
                            slant = (SKFontStyleSlant)reader.GetInt32();
                            break;
                    }
                }
            }

            throw new JsonException("Unexpected end when reading SKTypeface.");
        }

        throw new JsonException("Expected a JSON object or string for SKTypeface.");
    }

    public override void Write(Utf8JsonWriter writer, SKTypeface value, JsonSerializerOptions options)
    {
        SKFontStyle fontStyle = value.FontStyle;

        // If using default values, serialize only the font family name.
        if (fontStyle.Weight == (int)SKFontStyleWeight.Normal &&
            fontStyle.Slant == SKFontStyleSlant.Upright &&
            fontStyle.Width == (int)SKFontStyleWidth.Normal)
        {
            writer.WriteStringValue(value.FamilyName);
        }
        else
        {
            writer.WriteStartObject();
            writer.WriteString("familyName", value.FamilyName);
            writer.WriteNumber("weight", fontStyle.Weight);
            writer.WriteNumber("width", fontStyle.Width);
            writer.WriteNumber("slant", (int)fontStyle.Slant);
            writer.WriteEndObject();
        }
    }
}