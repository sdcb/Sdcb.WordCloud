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
        options.Mask?.FillMaskCache(options.Width, options.Height, cache);
        UpdateIntegral(cache, integral, options.Width, options.Height);

        // init canvas
        using SKCanvas canvas = new(result);
        DrawBackground(options.Width, options.Height, options.Background, canvas);

        foreach (WordFrequency word in options.WordFrequencies)
        {

        }
        throw new NotImplementedException();
    }

    internal unsafe static int[] UpdateIntegral(bool[] cache, int[] integral, int width, int height)
    {
        if (cache.Length != width * height || integral.Length != width * height)
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
            return integral;
        }
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
    }
