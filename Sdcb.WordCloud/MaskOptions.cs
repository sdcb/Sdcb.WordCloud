using SkiaSharp;
using System;

namespace Sdcb.WordClouds;

public record MaskOptions(SKBitmap Mask, SKColor? AllowedFillColor, SKColor? BlockedFillColor)
{
    public MaskOptions(SKBitmap Mask) : this(Mask, AllowedFillColor: default(SKColor), BlockedFillColor: null)
    {
    }

    public static MaskOptions CreateWithAllowedFillColor(SKBitmap mask, SKColor allowedFillColor)
    {
        return new(mask, allowedFillColor, null);
    }

    public static MaskOptions CreateWithBlockedFillColor(SKBitmap mask, SKColor blockedFillColor)
    {
        return new(mask, null, blockedFillColor);
    }

    internal unsafe void FillMaskCache(int width, int height, bool[] cache)
    {
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
            U8MaskToCache(cache, width, height, this);
        }
        else
        {
            using SKBitmap tempMask = ConvertColor(Mask, SKColorType.Gray8, SKAlphaType.Unknown);
            U8MaskToCache(cache, width, height, this with { Mask = tempMask });
        }
    }

    private static byte ConvertSKColorToGray8(SKColor color, SKColorType maskColorType)
    {
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

    private unsafe static bool[] U8MaskToCache(bool[] cache, int width, int height, MaskOptions mask)
    {
        byte* ptr = (byte*)mask.Mask.GetPixels();
        fixed (bool* dest = cache)
        {
            int size = width * height;
            if (mask.AllowedFillColor.HasValue)
            {
                byte gray8 = ConvertSKColorToGray8(mask.AllowedFillColor.Value, mask.Mask.ColorType);
                for (int i = 0; i < size; i++)
                {
                    dest[i] = ptr[i] != gray8;
                }
            }
            else if (mask.BlockedFillColor != null)
            {
                byte gray8 = ConvertSKColorToGray8(mask.BlockedFillColor.Value, mask.Mask.ColorType);
                for (int i = 0; i < size; i++)
                {
                    dest[i] = ptr[i] == gray8;
                }
            }
        }

        return cache;
    }

    private static SKBitmap ConvertColor(SKBitmap bmp, SKColorType colorType, SKAlphaType alohaType)
    {
        SKBitmap result = new(bmp.Width, bmp.Height, colorType, alohaType);
        using SKCanvas canvas = new(result);
        canvas.DrawBitmap(bmp, 0, 0);

        return result;
    }
}
