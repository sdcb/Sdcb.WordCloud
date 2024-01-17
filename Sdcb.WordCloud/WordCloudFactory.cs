using SkiaSharp;
using System;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Sdcb.WordCloud2.Tests")]

namespace Sdcb.WordClouds;

public static class WordCloudFactory
{
    public static SKBitmap Make(WordCloudOptions options)
    {
        SKBitmap result = new(options.Width, options.Height);
        int[,] integral = new int[options.Height, options.Width];
        bool[,] cache = new bool[options.Height, options.Width];
        options.Mask?.FillMaskCache(cache);
        UpdateIntegral(cache, integral);

        // init canvas
        using SKCanvas canvas = new(result);
        if (options.Background is not null)
        {
            if (options.Background.Width < options.Width || options.Background.Height < options.Height)
            {
                throw new ArgumentException("Background image size does not match the canvas size.");
            }
            canvas.DrawBitmap(options.Background, SKRect.Create(options.Width, options.Height));
        }

        float fontSize = options.GetInitialFontSize();
        using SKPaint paint = new()
        {
            IsAntialias = true,
        };
        foreach (WordFrequency word in options.WordFrequencies)
        {
            fontSize = options.FontSizeAccessor(new(word.Word, word.Frequency, fontSize));
        }
        throw new NotImplementedException();
    }

    internal unsafe static void UpdateIntegral(bool[,] cache, int[,] integral)
    {
        int height = cache.GetLength(0);
        int width = cache.GetLength(1);
        if (integral.GetLength(0) != height || integral.GetLength(1) != width)
        {
            throw new ArgumentException("Cache size does not match the integral size.");
        }

        fixed (bool* cachePtr = cache)
        fixed (int* integralPtr = integral)
        {
            // 初始化第一个元素
            integralPtr[0] = cachePtr[0] ? 1 : 0;

            // 初始化第一行
            for (int x = 1; x < width; x++)
            {
                integralPtr[x] = integralPtr[x - 1] + (cachePtr[x] ? 1 : 0);
            }

            // 初始化第一列
            for (int y = 1; y < height; y++)
            {
                int idx = y * width;
                integralPtr[idx] = integralPtr[idx - width] + (cachePtr[idx] ? 1 : 0);
            }

            // 计算其余的积分图值
            for (int y = 1; y < height; y++)
            {
                for (int x = 1; x < width; x++)
                {
                    int idx = y * width + x;
                    // 当前点的积分值 = 左边的积分值 + 上边的积分值 - 左上角的积分值 + 当前点的值
                    integralPtr[idx] = integralPtr[idx - 1] + integralPtr[idx - width]
                                        - integralPtr[idx - width - 1] + (cachePtr[idx] ? 1 : 0);
                }
            }
        }
    }
}
