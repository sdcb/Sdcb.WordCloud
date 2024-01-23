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
        PositionedTextGroup group = new (options.FontManager.GroupTextSingleLinePositioned(word.Word, fontPaintCache).ToArray());
        WordCloudContext ctx = new(options.Random, word.Word, word.Frequency, fontSize);
        foreach (SKPointI p in TraversePointsSequentially(options.Size, options.GetRandomStartPoint()))
        {
            List<(TextOrientations, SKRectI)> supportedAngles = new(capacity: 2);
            if (options.TextOrientation.HasFlag(TextOrientations.Horizontal))
            {
                supportedAngles.Add((TextOrientations.Horizontal, ExpandHorizontally(p, (int)group.Width, (int)group.Height)));
            }
            if (options.TextOrientation.HasFlag(TextOrientations.Vertical))
            {
                supportedAngles.Add((TextOrientations.Vertical, ExpandVertically(p, (int)group.Width, (int)group.Height)));
            }

            foreach ((TextOrientations orientation, SKRectI rect) in supportedAngles)
            {
                if (rect.Right >= options.Width || rect.Bottom >= options.Height || rect.Left < 0 || rect.Top < 0)
                {
                    continue;
                }
                if (integralMap.GetSum(rect) > 0)
                {
                    continue;
                }

#pragma warning disable CS8524 // switch 表达式不会处理其输入类型的某些值(它不是穷举)，这包括未命名的枚举值。
                TextItem result = new(word.Word, fontSize, options.FontColorAccessor(ctx), p, orientation switch
                {
                    TextOrientations.Horizontal => 0,
                    TextOrientations.Vertical => 90,
                });
#pragma warning restore CS8524 // switch 表达式不会处理其输入类型的某些值(它不是穷举)，这包括未命名的枚举值。

                using SKBitmap textLayout = group.CreateTextLayout(fontPaintCache);
                FillCache(textLayout, orientation, cache, rect);
                integralMap.Update(cache);
                return result;
            }
        }

        return null;
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

    internal static SKRectI ExpandHorizontally(SKPointI center, int width, int height)
    {
        int halfWidth = width / 2;
        int otherHalfWidth = width - halfWidth;
        int halfHeight = height / 2;
        int otherHalfHeight = height - halfHeight;
        // 宽度在水平方向上分布，因此需要调整中心点的X坐标。
        // 高度在垂直方向上均匀分布，因此中心点的Y坐标上下均匀分布halfHeight。
        return new SKRectI(center.X - halfWidth, center.Y - halfHeight, center.X + otherHalfWidth, center.Y + otherHalfHeight);
    }

    internal static SKRectI ExpandVertically(SKPointI center, int width, int height)
    {
        int halfHeight = height / 2;
        int otherHalfHeight = height - halfHeight;
        int halfWidth = width / 2;
        int otherHalfWidth = width - halfWidth;
        // 高度在垂直方向上分布，因此需要调整中心点的Y坐标。
        // 宽度在水平方向上均匀分布，因此中心点的X坐标左右均匀分布halfWidth。
        return new SKRectI(center.X - halfWidth, center.Y - halfHeight, center.X + otherHalfWidth, center.Y + otherHalfHeight);
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
