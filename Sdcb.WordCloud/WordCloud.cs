using SkiaSharp;
using System;
using System.IO;
using System.Linq;

namespace Sdcb.WordClouds;

public record WordCloud(int Width, int Height, FontManager FontManager, TextItem[] TextItems, SKBitmap? Background = null)
{
    public SKBitmap ToBitmap(bool addBox = false)
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
        throw new NotImplementedException();
    }
}

public record TextItem(string TextContent, float FontSize, SKColor Color, SKPoint Center, float Rotate);