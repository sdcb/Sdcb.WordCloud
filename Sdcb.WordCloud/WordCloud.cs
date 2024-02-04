using SkiaSharp;
using System;
using System.Globalization;
using System.IO;
using System.Text;

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
            using SKBitmap textLayout = line.CreateTextLayout(textPainter);

            SKSize size = line.Size;
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

    private static void TestWordCloudBitmap(SKBitmap bitmap, string fileName)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(Path.Combine(@"C:\Users\ZhouJie\source\repos\Sdcb.WordCloud\Sdcb.WordCloud2.Tests\bin\Debug\net8.0\WordCloudOutputs", fileName));
        data.SaveTo(stream);
    }

    public string ToSvg()
    {
        throw new NotImplementedException();
    }

    // Utility method to convert an SKColor to a valid CSS color string
    private static string SKColorToCSSColor(SKColor color)
    {
        return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}{(color.Alpha == 255 ? "" : $"{color.Alpha:X2}")}";
    }
}

public record TextLine(PositionedText[] Texts, float FontSize, SKColor Color, SKPoint Center, float Rotate) : PositionedTextGroup(Texts)
{
}