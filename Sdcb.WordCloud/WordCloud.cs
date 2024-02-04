using SkiaSharp;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
[assembly: InternalsVisibleTo("Sdcb.WordCloud.Tests")]

namespace Sdcb.WordClouds;

/// <summary>
/// Represents a word cloud graphic that can be rendered to a bitmap or converted to different graphic formats like SVG.
/// </summary>
public record WordCloud(int Width, int Height, TextLine[] TextLines, SKBitmap? Background = null)
{
    /// <summary>
    /// Renders the word cloud to an SKBitmap.
    /// </summary>
    /// <param name="addBox">If set to true, adds a box around each text line for visual debugging.</param>
    /// <returns>A new SKBitmap instance containing the rendered word cloud.</returns>
    /// <exception cref="ArgumentException">Thrown when the background image does not match the size of the canvas.</exception>
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

    /// <summary>
    /// Serializes the word cloud to a JSON string, using camel case property naming and indented formatting.
    /// </summary>
    /// <returns>A JSON string representing the word cloud.</returns>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
    }

    /// <summary>
    /// Deserializes a JSON string to a WordCloud object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A WordCloud object represented by the JSON string.</returns>
    public static WordCloud FromJson(string json)
    {
        return JsonSerializer.Deserialize<WordCloud>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        })!;
    }

    /// <summary>
    /// Converts the word cloud to an SVG (Scalable Vector Graphics) format.
    /// </summary>
    /// <returns>A string containing the SVG representation of the word cloud.</returns>
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

    /// <summary>
    /// Creates a new WordCloud instance based on the specified WordCloudOptions.
    /// </summary>
    /// <param name="options">A set of options that dictate how the word cloud should be constructed.</param>
    /// <returns>A new WordCloud instance built according to the provided options.</returns>
    public static WordCloud Create(WordCloudOptions options)
    {
        IntegralMap integralMap = new(options.Width, options.Height);
        bool[,] cache = new bool[options.Height, options.Width];
        options.Mask?.FillMaskCache(cache);
        integralMap.Update(cache);

        // init canvas
        float fontSize = options.GetInitialFontSize();

        using SKPaint fontPaintCache = new() { IsAntialias = false };
        Traverser traverser = TraverserHelper.CreateTraverser(options.TextOrientation, options.Random);
        TextLine[] items = options.WordScores
            .Select(word =>
            {
                TextLine? item = null;
                fontSize = options.FontSizeAccessor(new(options.Random, word.Word, word.Score, fontSize));
                do
                {
                    if (fontSize <= options.MinFontSize)
                    {
                        break;
                    }
                    item = CreateTextItem(options, traverser, integralMap, fontSize, cache, fontPaintCache, word);
                    if (item is null)
                    {
                        fontSize -= options.FontStep;
                    }
                } while (item is null);

                return item!;
            })
            .Where(item => item is not null)
            .ToArray();

        return new WordCloud(options.Width, options.Height, items, options.Background);
    }

    private static TextLine? CreateTextItem(WordCloudOptions options, Traverser traverser, IntegralMap integralMap, float fontSize, bool[,] cache, SKPaint fontPaintCache, WordScore word)
    {
        fontPaintCache.TextSize = fontSize;
        PositionedTextGroup group = new(options.FontManager.GroupTextSingleLinePositioned(word.Word, fontPaintCache).ToArray());
        WordCloudContext ctx = new(options.Random, word.Word, word.Score, fontSize);
        OrientationRect? orp = traverser(options.GetRandomStartPoint(), group.SizeI, integralMap);
        if (orp is null)
        {
            return null;
        }

        return FillAndUpdate(options, integralMap, fontSize, cache, fontPaintCache, group, ctx, orp.Value);
    }

    private static TextLine FillAndUpdate(WordCloudOptions options, IntegralMap integralMap, float fontSize, bool[,] cache, SKPaint fontPaintCache, PositionedTextGroup group, WordCloudContext ctx, OrientationRect orp)
    {
        using SKBitmap textLayout = group.CreateTextLayout(fontPaintCache);
        FillCache(textLayout, orp.Orientations, cache, orp.Rect);
        integralMap.Update(cache);
        return new TextLine(group, fontSize, options.FontColorAccessor(ctx), orp.Center, orp.ToDegree());
    }

    /// <summary>
    /// Fills a cache array with boolean values representing the presence of alpha channel from
    /// an SKBitmap image, considering the image's text orientation. The cache array is expected
    /// to be large enough to accommodate the SKBitmap's size. For vertical text orientation, the
    /// cache size is expected to have its width and height swapped relative to the SKBitmap size.
    /// </summary>
    /// <param name="bmp">The SKBitmap image with pixel data to transfer to the boolean cache.</param>
    /// <param name="destRect">The SKRectI defining the region within the cache to be filled. This is not 
    /// the crop area from the bitmap but the rectangle's position in the cache.</param>
    /// <param name="textOrientations">The text orientation of the bitmap, determining how the
    /// pixel data will be transferred to the boolean cache.</param>
    /// <param name="dest">The two-dimensional array to be filled with boolean values indicating presence
    /// (true) or absence (false) of the alpha channel for each pixel in the SKBitmap.</param>
    /// <exception cref="ArgumentException">Thrown when the provided SKBitmap is not of SKColorType.Bgra8888 color type,
    /// or if the cache array is not big enough to contain the specified SKRectI region.</exception>
    /// <remarks>
    /// This method directly maps pixels from a SKBitmap into a boolean array ('cache'),
    /// where a 'true' value indicates a non-transparent pixel (based on the alpha channel), and 
    /// a 'false' value indicates a transparent pixel. The cache should be pre-allocated
    /// and sized appropriately depending on the 'textOrientation' parameter -- it should either
    /// match the SKBitmap's dimensions for horizontal text orientation or have its dimensions 
    /// swapped for vertical text orientation. The bitmap must be Bgra8888, which supports an alpha channel.
    /// </remarks>
    internal static unsafe void FillCache(SKBitmap bmp, HorizontalOrVertical textOrientations, bool[,] dest, SKRectI destRect)
    {
        // Check if bmp is Bgra8888
        if (bmp.ColorType != SKColorType.Bgra8888)
        {
            throw new ArgumentException("Bitmap must be of type Bgra8888.", nameof(bmp));
        }

        int srcWidth = bmp.Width;
        int srcHeight = bmp.Height;
        int destHeight = dest.GetLength(0);
        int destWidth = dest.GetLength(1);

        // Check if the cache array is big enough to hold the rectangle
        if (destRect.Bottom > destHeight || destRect.Right > destWidth)
        {
            throw new ArgumentException("The cache array is not big enough to hold the rectangle.", nameof(dest));
        }

        uint* srcPtr = (uint*)bmp.GetPixels();
        fixed (bool* destPtr = dest)
        {
            if (textOrientations == HorizontalOrVertical.Horizontal)
            {
                for (int y = destRect.Top; y < destRect.Bottom; ++y)
                {
                    int srcRow = (y - destRect.Top) * srcWidth;
                    int destRow = y * destWidth;
                    for (int x = destRect.Left; x < destRect.Right; ++x)
                    {
                        int srcColumn = x - destRect.Left;
                        if ((srcPtr[srcRow + srcColumn] & 0xFF000000) > 0) // Use mask for Alpha channel
                        {
                            destPtr[destRow + x] = true;
                        }
                    }
                }
            }
            else if (textOrientations == HorizontalOrVertical.Vertical)
            {
                for (int y = destRect.Top; y < destRect.Bottom; ++y)
                {
                    int destRow = y * destWidth;
                    int srcColumn = y - destRect.Top;
                    for (int x = destRect.Left; x < destRect.Right; ++x)
                    {
                        int srcRow = (srcHeight - (x - destRect.Left + 1)) * srcWidth;
                        if ((srcPtr[srcRow + srcColumn] & 0xff000000) > 0) // Use mask for Alpha channel
                        {
                            destPtr[destRow + x] = true;
                        }
                    }
                }
            }
        }
    }
}
