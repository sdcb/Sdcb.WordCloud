using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Sdcb.WordCloud2.Tests")]

namespace Sdcb.WordClouds;

public static class WordCloudFactory
{
    public static WordCloud Make(WordCloudOptions options)
    {
        IntegralMap integralMap = new(options.Width, options.Height);
        bool[,] cache = new bool[options.Height, options.Width];
        options.Mask?.FillMaskCache(cache);
        integralMap.Update(cache);

        // init canvas
        float fontSize = options.GetInitialFontSize();

        using SKPaint fontPaintCache = new() { IsAntialias = false };
        TextItem[] items = options.WordFrequencies
            .Select(word =>
            {
                TextItem? item = null;
                fontSize = options.FontSizeAccessor(new(options.Random, word.Word, word.Frequency, fontSize));
                do
                {
                    if (fontSize <= options.MinFontSize)
                    {
                        break;
                    }
                    item = CreateTextItem(options, integralMap, fontSize, cache, fontPaintCache, word);
                    if (item is null)
                    {
                        fontSize -= options.FontStep;
                    }
                } while (item is null);

                return item!;
            })
            .Where(item => item is not null)
            .ToArray();

        return new WordCloud(options.Width, options.Height, options.FontManager, items, options.Background);
    }

    private static TextItem? CreateTextItem(WordCloudOptions options, IntegralMap integralMap, float fontSize, bool[,] cache, SKPaint fontPaintCache, WordFrequency word)
    {
        fontPaintCache.TextSize = fontSize;
        PositionedTextGroup group = new(options.FontManager.GroupTextSingleLinePositioned(word.Word, fontPaintCache).ToArray());
        WordCloudContext ctx = new(options.Random, word.Word, word.Frequency, fontSize);
        OrientationRect? orp = FindSuitableOrietationRect(options.GetRandomStartPoint(), group.SizeI, options.TextOrientation, integralMap);
        if (orp is null)
        {
            return null;
        }

        return FillAndUpdate(options, integralMap, fontSize, cache, fontPaintCache, word, group, ctx, orp.Value);
    }

    private static TextItem FillAndUpdate(WordCloudOptions options, IntegralMap integralMap, float fontSize, bool[,] cache, SKPaint fontPaintCache, WordFrequency word, PositionedTextGroup group, WordCloudContext ctx, OrientationRect orp)
    {
        TextItem result = new(word.Word, fontSize, options.FontColorAccessor(ctx), orp.Center, orp.ToDegree());
        SKBitmap textLayout = group.CreateTextLayout(fontPaintCache);
        FillCache(textLayout, orp.Orientations, cache, orp.Rect);
        integralMap.Update(cache);
        return result;
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
    /// <param name="textOrientation">The text orientation of the bitmap, determining how the
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
    internal static unsafe void FillCache(SKBitmap bmp, TextOrientations textOrientations, bool[,] dest, SKRectI destRect)
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
            if (textOrientations == TextOrientations.Horizontal)
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
            else if (textOrientations == TextOrientations.Vertical)
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

    internal static OrientationRect? FindSuitableOrietationRect(
        SKPointI startPoint,
        SKSizeI rectSize,
        TextOrientations allowedOrientations,
        IntegralMap integralMap)
    {
        // Ensure the start point is within bounds
        if (startPoint.X < 0 || startPoint.X >= integralMap.Width ||
            startPoint.Y < 0 || startPoint.Y >= integralMap.Height)
        {
            throw new ArgumentOutOfRangeException(nameof(startPoint));
        }

        // Begin traversal at startPoint
        SKPointI currentPoint = startPoint;

        // Continue indefinitely until we loop back to the start point
        do
        {
            // Yield the current point
            if (allowedOrientations.HasFlag(TextOrientations.Horizontal))
            {
                OrientationRect orp = OrientationRect.ExpandHorizontally(currentPoint, rectSize);
                if (orp.Rect.Right < integralMap.Width && orp.Rect.Bottom < integralMap.Height && orp.Rect.Left >= 0 && orp.Rect.Top >= 0 && integralMap.GetSum(orp.Rect) <= 0)
                {
                    return orp;
                }
            }
            if (allowedOrientations.HasFlag(TextOrientations.Vertical))
            {
                OrientationRect orp = OrientationRect.ExpandVertically(currentPoint, rectSize);
                if (orp.Rect.Right < integralMap.Width && orp.Rect.Bottom < integralMap.Height && orp.Rect.Left >= 0 && orp.Rect.Top >= 0 && integralMap.GetSum(orp.Rect) <= 0)
                {
                    return orp;
                }
            }

            // Move to the next point
            currentPoint.X++;

            // If we reach the end of the row, move to the next row
            if (currentPoint.X >= integralMap.Width)
            {
                currentPoint.X = 0;
                currentPoint.Y++;
            }

            // If we reach the end of the columns, start back at the top
            if (currentPoint.Y >= integralMap.Height)
            {
                currentPoint.Y = 0;
            }

            // If after wrapping around we're at the start point, stop traversing
        } while (currentPoint.X != startPoint.X || currentPoint.Y != startPoint.Y);

        return null;
    }

    internal static IEnumerable<SKPointI> TraversePointsSequentially(SKSizeI maxSize, SKPointI startPoint)
    {
        if (maxSize == SKSizeI.Empty)
        {
            yield break;
        }

        // Ensure the start point is within bounds
        if (startPoint.X < 0 || startPoint.X >= maxSize.Width ||
            startPoint.Y < 0 || startPoint.Y >= maxSize.Height)
        {
            throw new ArgumentOutOfRangeException(nameof(startPoint));
        }

        // Begin traversal at startPoint
        SKPointI currPoint = startPoint;

        // Continue indefinitely until we loop back to the start point
        do
        {
            // Yield the current point
            yield return currPoint;

            // Move to the next point
            currPoint.X++;

            // If we reach the end of the row, move to the next row
            if (currPoint.X >= maxSize.Width)
            {
                currPoint.X = 0;
                currPoint.Y++;
            }

            // If we reach the end of the columns, start back at the top
            if (currPoint.Y >= maxSize.Height)
            {
                currPoint.Y = 0;
            }

            // If after wrapping around we're at the start point, stop traversing
        } while (currPoint.X != startPoint.X || currPoint.Y != startPoint.Y);
    }
}
