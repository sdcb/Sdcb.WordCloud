using SkiaSharp;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Sdcb.WordClouds;

public record WordCloud(int Width, int Height, TextLine[] TextLines, SKBitmap? Background = null)
{
    public SKBitmap ToSKBitmap(bool addBox = false)
    {
        SKBitmap result = new(Width, Height);
        using SKCanvas canvas = new(result);

        if (Background is not null)
        {
            if (Background.Width < Width || Background.Height < Height)
            {
                throw new ArgumentException("Background image size does not match the canvas size.");
            }
            canvas.DrawBitmap(Background, SKRect.Create(Width, Height));
        }

        using SKPaint textPainter = new() { IsAntialias = true, };
        foreach (TextLine line in TextLines)
        {
            textPainter.TextSize = line.FontSize;
            textPainter.Color = line.Color;
            using SKBitmap textLayout = line.TextGroup.CreateTextLayout(textPainter);

            SKSize size = line.TextGroup.Size;
            SKPoint startPoint = new(line.Center.X - size.Width / 2, line.Center.Y - size.Height / 2);

            //TestWordCloudBitmap(temp, item.TextContent + ".png");
            SKPoint realCenter = new(size.Width / 2, size.Height / 2);
            SKMatrix transform = SKMatrix.Concat(
                SKMatrix.CreateTranslation(startPoint.X, startPoint.Y),
                SKMatrix.CreateRotationDegrees(line.Rotate, realCenter.X, realCenter.Y)
                );

            canvas.SetMatrix(transform);
            canvas.DrawBitmap(textLayout, SKPoint.Empty);
            if (addBox)
            {
                SKRect destBox = SKRect.Create(size);
                canvas.DrawRect(destBox, new SKPaint { Color = line.Color, Style = SKPaintStyle.Stroke, StrokeWidth = 1 });
            }
        }
        return result;
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
    }

    public static WordCloud FromJson(string json)
    {
        return JsonSerializer.Deserialize<WordCloud>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        })!;
    }

    public string ToSvg()
    {
        // SVG header with necessary namespaces and initial viewport
        StringBuilder svgBuilder = new();
        svgBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>");
        svgBuilder.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{Width}\" height=\"{Height}\" viewBox=\"0 0 {Width} {Height}\">");

        // Drawing each word as text in SVG
        foreach (TextLine line in TextLines)
        {
            // To handle rotation and positioning, we use a group with a transform attribute
            svgBuilder.AppendLine($"<g transform=\"translate({line.Center.X},{line.Center.Y}) rotate({line.Rotate})\">");

            // Adding each part of the text line
            foreach (PositionedText positionedText in line.TextGroup.Texts)
            {
                // Convert the color to a CSS compatible format
                string color = SKColorJsonConverter.SKColorToCSSColor(line.Color);

                // Adding text element with applied styles
                svgBuilder.AppendLine(
                    $"<text x=\"{-line.TextGroup.Size.Width / 2}\" y=\"{positionedText.Width - line.TextGroup.Size.Width / 2 - positionedText.Left}\" " +
                    $"fill=\"{color}\" font-family=\"{positionedText.Typeface.FamilyName}\" font-size=\"{line.FontSize}px\">" +
                    $"{System.Security.SecurityElement.Escape(positionedText.Text)}</text>");
            }

            svgBuilder.AppendLine("</g>"); // Close the group
        }

        // Close the SVG tag and return the SVG content as a string
        svgBuilder.AppendLine("</svg>");
        return svgBuilder.ToString();
    }
}
