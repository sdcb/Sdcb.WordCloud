using SkiaSharp;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Sdcb.WordClouds;

public record WordCloud(int Width, int Height, FontManager FontManager, TextItem[] TextItems, SKBitmap? Background = null)
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
        foreach (TextItem item in TextItems)
        {
            if (string.IsNullOrEmpty(item.TextContent))
            {
                continue;
            }

            textPainter.TextSize = item.FontSize;
            textPainter.Color = item.Color;
            using SKBitmap temp = FontManager.CreateTextLayout(item.TextContent, textPainter);

            SKSize size = new(temp.Width, temp.Height);
            SKPoint topLeft = new(item.Center.X - size.Width / 2, item.Center.Y - size.Height / 2);

            //TestWordCloudBitmap(temp, item.TextContent + ".png");
            SKPoint realCenter = new(size.Width / 2, size.Height / 2);
            SKMatrix transform = SKMatrix.Concat(
                SKMatrix.CreateTranslation(topLeft.X, topLeft.Y),
                SKMatrix.CreateRotationDegrees(item.Rotate, realCenter.X, realCenter.Y)
                );

            canvas.SetMatrix(transform);
            canvas.DrawBitmap(temp, SKPoint.Empty);
            if (addBox)
            {
                SKRect destBox = SKRect.Create(size);
                canvas.DrawRect(destBox, new SKPaint { Color = item.Color, Style = SKPaintStyle.Stroke, StrokeWidth = 1 });
            }
        }
        return result;
    }

    private static void TestWordCloudBitmap(SKBitmap bitmap, string fileName)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(Path.Combine(@"C:\Users\ZhouJie\source\repos\Sdcb.WordCloud\Sdcb.WordCloud2.Tests\bin\Debug\net8.0\WordCloudOutputs", fileName));
        data.SaveTo(stream);
    }

    public string ToSvg()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" standalone=\"no\"?>");
        sb.AppendLine("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">");
        sb.AppendLine($"<svg width=\"{Width}px\" height=\"{Height}px\" version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\">");

        // If there is a background image, convert it to a base64-encoded string and include it in the SVG
        if (Background is not null)
        {
            using var image = SKImage.FromBitmap(Background);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            string base64 = Convert.ToBase64String(data.ToArray());
            sb.AppendLine($"<image width=\"{Width}\" height=\"{Height}\" href=\"data:image/png;base64,{base64}\" />");
        }

        foreach (TextItem item in TextItems)
        {
            if (string.IsNullOrEmpty(item.TextContent))
            {
                continue;
            }

            string fontSize = item.FontSize.ToString("0.###", CultureInfo.InvariantCulture);
            string color = SKColorToCSSColor(item.Color);
            string content = item.TextContent.Replace("&", "&amp;") // Replace & with &amp;
                                           .Replace("<", "&lt;")    // Replace < with &lt;
                                           .Replace(">", "&gt;");   // Replace > with &gt;

            // Calculate the transform for the position and rotation of the text
            float x = item.Center.X;
            float y = item.Center.Y;
            string transform = "";
            if (item.Rotate != 0)
            {
                transform = $" transform=\"rotate({item.Rotate.ToString(CultureInfo.InvariantCulture)}, {x.ToString(CultureInfo.InvariantCulture)}, {y.ToString(CultureInfo.InvariantCulture)})\"";
            }

            sb.AppendLine($"<text x=\"{x.ToString(CultureInfo.InvariantCulture)}\" y=\"{y.ToString(CultureInfo.InvariantCulture)}\" font-size=\"{fontSize}\" fill=\"{color}\"{transform}>{content}</text>");
        }

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    // Utility method to convert an SKColor to a valid CSS color string
    private static string SKColorToCSSColor(SKColor color)
    {
        return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}{(color.Alpha == 255 ? "" : $"{color.Alpha:X2}")}";
    }
}

public record TextItem(string TextContent, float FontSize, SKColor Color, SKPoint Center, float Rotate);