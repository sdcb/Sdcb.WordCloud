using SkiaSharp;
using System;

namespace Sdcb.WordClouds;

public class WordCloud
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

            if (mask.ColorType != SKColorType.Alpha8)
            {
                throw new ArgumentException("Mask image must be grayscale.");
            }

            byte* ptr = (byte*)mask.GetPixels();
            fixed (bool* dest = cache)
            {
                int size = width * height;
                for (int i = 0; i < size; i++)
                {
                    dest[i] = ptr[i] > 0;
                }
            }
        }
        return cache;
    }
}
