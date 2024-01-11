using SkiaSharp;
using System;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Sdcb.WordCloud2.Tests")]

namespace Sdcb.WordClouds;

public static class WordCloud
{
    public static SKBitmap Make(WordCloudOptions options)
    {
        SKBitmap result = new(options.Width, options.Height);
        int[] integral = new int[options.Height * options.Width];
        bool[] cache = new bool[options.Height * options.Width];

        // init canvas
        using SKCanvas canvas = new(result);
        DrawBackground(options.Width, options.Height, options.Background, canvas);

        foreach (WordFrequency word in options.WordFrequencies)
        {

        }
        throw new NotImplementedException();
    }

    private static void DrawBackground(int width, int height, SKBitmap? background, SKCanvas canvas)
    {
        // draw background if provided
        if (background is not null)
        {
            if (background.Width < width || background.Height < height)
            {
                throw new ArgumentException("Background image size does not match the canvas size.");
            }
            canvas.DrawBitmap(background, SKRect.Create(width, height));
        }
    }

    internal unsafe static bool[] CreateMaskCache(int width, int height, SKBitmap? mask)
    {
        bool[] cache = new bool[width * height];
        if (mask != null)
        {
            if (mask.Width < width || mask.Height < height)
            {
                throw new ArgumentException("Mask image size does not match the canvas size.");
            }

            if (mask.ColorType == SKColorType.Alpha8 || mask.ColorType == SKColorType.Gray8)
            {
                U8MaskToCache(cache, width, height, mask);
            }
            else
            {
                using SKBitmap tempMask = ConvertColor(mask, SKColorType.Gray8, SKAlphaType.Unknown);
                U8MaskToCache(cache, width, height, tempMask);
            }
        }
        return cache;

        static unsafe bool[] U8MaskToCache(bool[] cache, int width, int height, SKBitmap mask)
        {
            byte* ptr = (byte*)mask.GetPixels();
            fixed (bool* dest = cache)
            {
                int size = width * height;
                for (int i = 0; i < size; i++)
                {
                    dest[i] = ptr[i] > 0;
                }
            }

            return cache;
        }

        static SKBitmap ConvertColor(SKBitmap bmp, SKColorType colorType, SKAlphaType alohaType)
        {
            SKBitmap result = new(bmp.Width, bmp.Height, colorType, alohaType);
            using SKCanvas canvas = new(result);
            canvas.DrawBitmap(bmp, 0, 0);

            return result;
        }
    }
}
