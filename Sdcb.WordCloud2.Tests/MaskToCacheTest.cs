using Sdcb.WordClouds;
using SkiaSharp;
using System.Buffers.Binary;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sdcb.WordClouds.Tests;

public class MaskToCacheTest
{
    [Fact]
    public unsafe void Gray8Default()
    {
        int width = 8, height = 6;
        using SKBitmap bmp = new(width, height, SKColorType.Gray8, SKAlphaType.Unknown);
        byte* pbmp = (byte*)bmp.GetPixels();
        for (int i = 0; i < 5; ++i)
        {
            pbmp[Random.Shared.Next(width * height)] = (byte)Random.Shared.Next(1, 256);
        }

        byte[] bmpData = bmp.GetPixelSpan().ToArray();
        MaskOptions mask = new(bmp);

        bool[,] cache = new bool[height, width];
        mask.FillMaskCache(cache);
        Assert.Equal(width * height, cache.Length);
        Assert.Equal(bmpData.Select(x => x > 0), Utils.Convert2DTo1D(cache));
    }

    [Fact]
    public unsafe void Gray8_Black()
    {
        int width = 8, height = 6;
        using SKBitmap bmp = new(width, height, SKColorType.Gray8, SKAlphaType.Unknown);
        byte* pbmp = (byte*)bmp.GetPixels();
        for (int i = 0; i < 5; ++i)
        {
            pbmp[Random.Shared.Next(width * height)] = (byte)Random.Shared.Next(1, 256);
        }

        byte[] bmpData = bmp.GetPixelSpan().ToArray();
        MaskOptions mask = MaskOptions.CreateWithBackgroundColor(bmp, SKColors.Black);

        bool[,] cache = new bool[height, width];
        mask.FillMaskCache(cache);
        Assert.Equal(width * height, cache.Length);
        Assert.Equal(bmpData.Select(x => x > 0), Utils.Convert2DTo1D(cache));
    }

    [Fact]
    public unsafe void Alpha8()
    {
        int width = 8, height = 6;
        using SKBitmap bmp = new(width, height, SKColorType.Alpha8, SKAlphaType.Premul);
        byte* pbmp = (byte*)bmp.GetPixels();
        for (int i = 0; i < 5; ++i)
        {
            pbmp[Random.Shared.Next(width * height)] = (byte)Random.Shared.Next(1, 256);
        }
        MaskOptions mask = MaskOptions.CreateWithBackgroundColor(bmp, SKColors.Transparent);
        bool[,] cache = new bool[height, width];
        mask.FillMaskCache(cache);

        byte[] bmpData = bmp.GetPixelSpan().ToArray();
        Assert.Equal(width * height, cache.Length);
        Assert.Equal(bmpData.Select(x => x > 0), Utils.Convert2DTo1D(cache));
    }

    [Fact]
    public unsafe void Transparent()
    {
        // arrange
        int width = 8, height = 6;
        using SKBitmap bmp = new(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        byte* pbmp = (byte*)bmp.GetPixels();
        for (int i = 0; i < 5; ++i)
        {
            int pixelId = Random.Shared.Next(width * height);
            pbmp[pixelId * 4 + Random.Shared.Next(0, 3)] = (byte)Random.Shared.Next(1, 256);
        }
        MaskOptions mask = MaskOptions.CreateWithBackgroundColor(bmp, SKColors.Transparent);

        bool[,] cache = new bool[height, width];

        // act
        mask.FillMaskCache(cache);

        // assert
        int[] bmpData = bmp.GetPixelSpan().ToArray().Chunk(4).Select(x => BitConverter.ToInt32(x)).ToArray();
        Assert.Equal(width * height, cache.Length);
        Assert.Equal(bmpData.Select(x => x > 0), Utils.Convert2DTo1D(cache));
    }

    [Fact]
    public unsafe void BackgroundBlack()
    {
        // arrange
        int width = 8, height = 6;
        using SKBitmap bmp = new(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using (SKCanvas canvas = new(bmp))
        {
            canvas.Clear(SKColors.Black);
        }

        byte* pbmp = (byte*)bmp.GetPixels();
        for (int i = 0; i < 5; ++i)
        {
            int pixelId = Random.Shared.Next(width * height);
            pbmp[pixelId * 4 + Random.Shared.Next(0, 3)] = (byte)Random.Shared.Next(1, 256);
        }
        MaskOptions mask = MaskOptions.CreateWithBackgroundColor(bmp, SKColors.Black);

        bool[,] cache = new bool[height, width];

        // act
        mask.FillMaskCache(cache);

        // assert
        int[] bmpData = bmp.GetPixelSpan().ToArray().Chunk(4)
            .Select(x => MaskOptions.ConvertSKColorToInt32(BinaryPrimitives.ReadUInt32LittleEndian(x), SKColorType.Bgra8888))
            .ToArray();
        int black = MaskOptions.ConvertSKColorToInt32(SKColors.Black, SKColorType.Bgra8888);
        Assert.Equal(width * height, cache.Length);
        Assert.Equal(bmpData.Select(x => x != black), Utils.Convert2DTo1D(cache));
    }

    [Fact]
    public unsafe void Bgra8888_Red()
    {
        // arrange
        int width = 8, height = 6;
        using SKBitmap bmp = new(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using (SKCanvas canvas = new(bmp))
        {
            canvas.Clear(SKColors.Red);
        }

        byte* pbmp = (byte*)bmp.GetPixels();
        for (int i = 0; i < 5; ++i)
        {
            int pixelId = Random.Shared.Next(width * height);
            pbmp[pixelId * 4 + Random.Shared.Next(0, 3)] = (byte)Random.Shared.Next(1, 254);
        }
        MaskOptions mask = MaskOptions.CreateWithBackgroundColor(bmp, SKColors.Red);

        bool[,] cache = new bool[height, width];

        // act
        mask.FillMaskCache(cache);

        // assert
        int[] bgraData = bmp.GetPixelSpan().ToArray().Chunk(4)
            .Select(x => MaskOptions.ConvertSKColorToInt32(BinaryPrimitives.ReadUInt32LittleEndian(x), SKColorType.Bgra8888))
            .ToArray();
        int backgroundBgra = MaskOptions.ConvertSKColorToInt32(SKColors.Red, SKColorType.Bgra8888);
        Assert.Equal(width * height, cache.Length);
        Assert.Equal(bgraData.Select(x => x != backgroundBgra), Utils.Convert2DTo1D(cache));
    }

    [Fact]
    public unsafe void Rgba8888_Red()
    {
        // arrange
        int width = 8, height = 6;
        using SKBitmap bmp = new(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using (SKCanvas canvas = new(bmp))
        {
            canvas.Clear(SKColors.Red);
        }

        byte* pbmp = (byte*)bmp.GetPixels();
        for (int i = 0; i < 5; ++i)
        {
            int pixelId = Random.Shared.Next(width * height);
            pbmp[pixelId * 4 + Random.Shared.Next(0, 3)] = (byte)Random.Shared.Next(1, 254);
        }
        MaskOptions mask = MaskOptions.CreateWithBackgroundColor(bmp, SKColors.Red);

        bool[,] cache = new bool[height, width];

        // act
        mask.FillMaskCache(cache);

        // assert
        int[] bmpData = bmp.GetPixelSpan().ToArray().Chunk(4)
            .Select(x => MaskOptions.ConvertSKColorToInt32(BinaryPrimitives.ReadUInt32LittleEndian(x), SKColorType.Rgba8888))
            .ToArray();
        int backgroundBgra = MaskOptions.ConvertSKColorToInt32(SKColors.Red, SKColorType.Bgra8888);
        Assert.Equal(width * height, cache.Length);
        Assert.Equal(bmpData.Select(x => x != backgroundBgra), Utils.Convert2DTo1D(cache));
    }

    [Fact]
    public unsafe void Rgba8888_Red_Foreground()
    {
        // arrange
        int width = 5, height = 3;
        using SKBitmap bmp = new(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        int foregroundColor = MaskOptions.ConvertSKColorToInt32(SKColors.Red, SKColorType.Rgba8888);
        int* pbmp = (int*)bmp.GetPixels();
        for (int i = 0; i < 5; ++i)
        {
            int pixelId = Random.Shared.Next(width * height);
            pbmp[pixelId] = foregroundColor;
        }
        MaskOptions mask = MaskOptions.CreateWithForegroundColor(bmp, SKColors.Red);
        bool[,] cache = new bool[height, width];

        // act
        mask.FillMaskCache(cache);

        // assert
        int[] bmpData = bmp.GetPixelSpan().ToArray().Chunk(4)
            .Select(x => (int)BinaryPrimitives.ReadUInt32LittleEndian(x))
            .ToArray();
        
        Assert.Equal(width * height, cache.Length);
        Assert.Equal(bmpData.Select(x => x == foregroundColor), Utils.Convert2DTo1D(cache));
    }

    [Fact]
    public unsafe void Rgba888x_Blue_Foreground()
    {
        // arrange
        int width = 5, height = 3;
        using SKBitmap bmp = new(width, height, SKColorType.Rgb888x, SKAlphaType.Premul);
        SKColor foregroundColor = SKColors.Blue;
        int foregroundColorI32 = MaskOptions.ConvertSKColorToInt32(foregroundColor, SKColorType.Rgb888x);
        int* pbmp = (int*)bmp.GetPixels();
        for (int i = 0; i < 5; ++i)
        {
            int pixelId = Random.Shared.Next(width * height);
            pbmp[pixelId] = foregroundColorI32;
        }
        MaskOptions mask = MaskOptions.CreateWithForegroundColor(bmp, foregroundColor);
        bool[,] cache = new bool[height, width];

        // act
        mask.FillMaskCache(cache);

        // assert
        int[] bmpData = bmp.GetPixelSpan().ToArray().Chunk(4)
            .Select(x => (int)BinaryPrimitives.ReadUInt32LittleEndian(x))
            .ToArray();

        Assert.Equal(width * height, cache.Length);
        Assert.Equal(bmpData.Select(x => x == foregroundColorI32), Utils.Convert2DTo1D(cache));
    }

    [Fact]
    public unsafe void Other_NotSupported()
    {
        int width = 5, height = 3;
        using SKBitmap bmp = new(width, height, SKColorType.Bgr101010x, SKAlphaType.Premul);
        MaskOptions mask = MaskOptions.CreateWithBackgroundColor(bmp, SKColors.Transparent);
        bool[,] cache = new bool[height, width];
        Assert.Throws<ArgumentException>(() => mask.FillMaskCache(cache));
    }
}