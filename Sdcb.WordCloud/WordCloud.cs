using SkiaSharp;
using System;
using System.IO;

namespace Sdcb.WordClouds;

public record WordCloud(int Width, int Height, string[] FontFamilyNames, TextItem[] TextItems)
{
    public SKBitmap ToBitmap(bool addBox = false)
    {
        SKBitmap result = new(Width, Height);
        using SKCanvas canvas = new(result);
        using SKPaint textPainter = new()
        {
            IsAntialias = true,
        };
        using FontManager fontManager = new(FontFamilyNames);
        using SKBitmap temp = new(Width, Height);
        using SKCanvas tempCanvas = new(temp);
        using SKPaint tempClearer = new()
        {
            BlendMode = SKBlendMode.Src,
            Color = SKColors.Transparent,
        };
        foreach (TextItem item in TextItems)
        {
            tempCanvas.DrawRect(SKRect.Create(item.Rect.Size), tempClearer);
            textPainter.TextSize = item.FontSize;
            textPainter.Color = item.Color;
            float x = 0;
            foreach (TextAndFont segment in fontManager.GroupTextSingleLine(item.TextContent))
            {
                textPainter.Typeface = segment.Typeface;
                tempCanvas.DrawText(item.TextContent, x, -textPainter.FontMetrics.Ascent, textPainter);
                x += textPainter.MeasureText(segment.Text);
            }

            //TestWordCloudBitmap(temp, item.TextContent + ".png");
            SKRect realRect = SKRect.Create(x, item.FontSize);
            SKRect destRect = SKRect.Create(realRect.Size);
            SKMatrix transform = SKMatrix.Concat(
                SKMatrix.CreateTranslation(item.Rect.Location.X, item.Rect.Location.Y),
                SKMatrix.CreateRotationDegrees(item.Rotate, 0, 0)
                //SKMatrix.Identity
                );
            canvas.SetMatrix(transform);
            canvas.DrawBitmap(temp, SKPoint.Empty);
            if (addBox)
            {
                canvas.DrawRect(destRect, new SKPaint { Color = item.Color, Style = SKPaintStyle.Stroke, StrokeWidth = 1 });
            }
        }
        return result;
    }

    private void TestWordCloudBitmap(SKBitmap bitmap, string fileName)
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

public record TextItem(string TextContent, float FontSize, SKColor Color, SKRect Rect, float Rotate);