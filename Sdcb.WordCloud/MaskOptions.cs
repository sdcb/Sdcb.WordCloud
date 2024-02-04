using SkiaSharp;
using System;

namespace Sdcb.WordClouds;

/// <summary>
/// Represents the mask options for creating a word cloud, specifying which areas of the mask are fillable.
/// </summary>
/// <remarks>
/// <para>The Mask property supports these pixel formats:</para>
/// <list type="bullet">
/// <item><see cref="SKColorType.Gray8"/></item>
/// <item><see cref="SKColorType.Alpha8"/></item>
/// <item><see cref="SKColorType.Bgra8888"/></item>
/// <item><see cref="SKColorType.Rgba8888"/></item>
/// <item><see cref="SKColorType.Rgb888x"/></item>
/// </list>
/// <para>Note that other pixel formats are not supported.</para>
/// <para>Either BackgroundColor or ForegroundColor can be set, but not both; if both are provided, only ForegroundColor will be used.</para>
/// <para>If BackgroundColor is not null, areas of the mask image that match the BackgroundColor are identified as fillable areas,
/// and any other color is considered non-fillable. This is suitable for scenarios where the background has been pre-marked.</para>
/// <para>If ForegroundColor is not null, then areas of the mask image that match the ForegroundColor are identified as 
/// non-fillable areas, and any other color is regarded as fillable. This is suitable for scenarios where the foreground has been pre-marked.</para>
/// </remarks>
/// <param name="Mask">The mask bitmap image to define fillable areas for word cloud generation.</param>
/// <param name="BackgroundColor">The color in the mask image representing the fillable areas; null to ignore this option.</param>
/// <param name="ForegroundColor">The color in the mask image representing non-fillable areas; null to ignore this option.</param>
public record MaskOptions(SKBitmap Mask, SKColor? BackgroundColor, SKColor? ForegroundColor)
{
    /// <summary>
    /// Initializes a new instance of the MaskOptions record with transparent background color.
    /// </summary>
    /// <param name="Mask">The mask bitmap image to define fillable areas for word cloud generation.</param>
    public MaskOptions(SKBitmap Mask) : this(Mask, BackgroundColor: SKColors.Transparent, ForegroundColor: null)
    {
    }

    /// <summary>
    /// Creates a new MaskOptions instance specifying a background color as the allowed fillable color.
    /// </summary>
    /// <param name="mask">The mask bitmap image to use for generating the word cloud.</param>
    /// <param name="backgroundColor">The color representing the fillable areas within the mask image.</param>
    /// <returns>A new MaskOptions record configured with the allowed background fill color.</returns>
    public static MaskOptions CreateWithBackgroundColor(SKBitmap mask, SKColor backgroundColor)
    {
        return new(mask, backgroundColor, null);
    }

    /// <summary>
    /// Creates a new MaskOptions instance specifying a foreground color as the blocked non-fillable color.
    /// </summary>
    /// <param name="mask">The mask bitmap image to use for generating the word cloud.</param>
    /// <param name="foregroundColor">The color representing the non-fillable areas within the mask image.</param>
    /// <returns>A new MaskOptions record configured with the specified foreground color to block filling.</returns>
    public static MaskOptions CreateWithForegroundColor(SKBitmap mask, SKColor foregroundColor)
    {
        return new(mask, null, foregroundColor);
    }

    /// <summary>
    /// Fills a two-dimensional boolean array that acts as a cache, indicating which points inside the word cloud can contain text, 
    /// based on the opacity of the mask at the corresponding positions.
    /// If the mask pixel is opaque, the cache value will be true (indicating available space); otherwise, it will be false.
    /// </summary>
    /// <param name="cache">The two-dimensional boolean array to be filled with mask data.</param>
    /// <exception cref="ArgumentException">Thrown when the mask image size is smaller than the cache's size or
    /// when the provided cache size does not match the expected size derived from its dimensions or
    /// when an unsupported mask color type is provided.</exception>
    internal void FillMaskCache(bool[,] cache)
    {
        int height = cache.GetLength(0);
        int width = cache.GetLength(1);
        if (Mask.Width < width || Mask.Height < height)
        {
            throw new ArgumentException("Mask image size does not match the canvas size.");
        }

        if (width * height != cache.Length)
        {
            throw new ArgumentException("Cache size does not match the canvas size.");
        }

        if (Mask.ColorType == SKColorType.Alpha8 || Mask.ColorType == SKColorType.Gray8)
        {
            FillMaskCacheU8(cache);
        }
        else if (Mask.ColorType == SKColorType.Bgra8888 || Mask.ColorType == SKColorType.Rgba8888 || Mask.ColorType == SKColorType.Rgb888x)
        {
            FillMaskCacheBgra8888(cache);
        }
        else
        {
            throw new ArgumentException("Mask color type must be Alpha8, Gray8, Bgra8888, Rgba8888, or Rgb888x.");
        }
    }

    internal unsafe void FillMaskCacheBgra8888(bool[,] cache)
    {
        int height = cache.GetLength(0);
        int width = cache.GetLength(1);
        int* ptr = (int*)Mask.GetPixels();
        fixed (bool* dest = cache)
        {
            int size = width * height;
            if (BackgroundColor.HasValue)
            {
                int i32 = ConvertSKColorToInt32(BackgroundColor.Value, Mask.ColorType);
                for (int i = 0; i < size; i++)
                {
                    dest[i] = ptr[i] != i32;
                }
            }
            else if (ForegroundColor != null)
            {
                int i32 = ConvertSKColorToInt32(ForegroundColor.Value, Mask.ColorType);
                for (int i = 0; i < size; i++)
                {
                    dest[i] = ptr[i] == i32;
                }
            }
        }
    }

    internal unsafe void FillMaskCacheU8(bool[,] cache)
    {
        int height = cache.GetLength(0);
        int width = cache.GetLength(1);
        byte* ptr = (byte*)Mask.GetPixels();
        fixed (bool* dest = cache)
        {
            int size = width * height;
            if (BackgroundColor.HasValue)
            {
                byte gray8 = ConvertSKColorToU8(BackgroundColor.Value, Mask.ColorType);
                for (int i = 0; i < size; i++)
                {
                    dest[i] = ptr[i] != gray8;
                }
            }
            else if (ForegroundColor != null)
            {
                byte gray8 = ConvertSKColorToU8(ForegroundColor.Value, Mask.ColorType);
                for (int i = 0; i < size; i++)
                {
                    dest[i] = ptr[i] == gray8;
                }
            }
        }
    }

    private static SKBitmap ConvertColor(SKBitmap bmp, SKColorType colorType, SKAlphaType alohaType)
    {
        SKBitmap result = new(bmp.Width, bmp.Height, colorType, alohaType);
        using SKCanvas canvas = new(result);
        canvas.DrawBitmap(bmp, 0, 0);

        return result;
    }

    internal static int ConvertSKColorToInt32(SKColor color, SKColorType maskColorType)
    {
        if (color.Alpha == 0) return 0;

        // 转换颜色格式，将 SKColor 拆分为对应的通道
        byte red = color.Red;
        byte green = color.Green;
        byte blue = color.Blue;
        byte alpha = color.Alpha;

        return maskColorType switch
        {
            SKColorType.Rgba8888 => (alpha << 24) | (blue << 16) | (green << 8) | red,// RGBA8888 格式的32位整数
            SKColorType.Bgra8888 => (alpha << 24) | (red << 16) | (green << 8) | blue,// BGRA8888 格式的32位整数
            SKColorType.Rgb888x => (0xFF << 24) | (blue << 16) | (green << 8) | red,// RGB888x 格式是没有Alpha通道的RGB颜色，x代表无用通道
            _ => throw new ArgumentOutOfRangeException(nameof(maskColorType),
                "Mask color type must be Rgba8888, Bgra8888, or Rgb888x."),// 如果不是上述指定的格式，则抛出异常
        };
    }

    private static byte ConvertSKColorToU8(SKColor color, SKColorType maskColorType)
    {
        if (color.Alpha == 0) return 0;

        if (maskColorType == SKColorType.Gray8)
        {
            // 根据亮度计算公式计算灰度值
            // Y' = 0.299 R + 0.587 G + 0.114 B
            float gray = 0.299f * color.Red + 0.587f * color.Green + 0.114f * color.Blue;

            // 将计算结果转换为byte
            return (byte)gray;
        }
        else if (maskColorType == SKColorType.Alpha8)
        {
            return color.Alpha;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(maskColorType), "Mask color type must be Gray8 or Alpha8.");
        }
    }
}
