using Sdcb.WordClouds;
using SkiaSharp;

namespace Sdcb.WordCloud2.Tests;

public class UnitTest1
{
    [Fact]
    public unsafe void CreateMaskCacheTest_Mask_Gray8()
    {
        int width = 8, height = 6;
        using SKBitmap bmp = new(width, height, SKColorType.Gray8, SKAlphaType.Unknown);
        byte* pbmp = (byte*)bmp.GetPixels();
        for (int i = 0; i < 5; ++i)
        {
            pbmp[Random.Shared.Next(width * height)] = (byte)Random.Shared.Next(1, 256);
        }
        pbmp[3] = 3;

        byte[] bmpData = bmp.GetPixelSpan().ToArray();
        MaskOptions mask = new(bmp);

        bool[] data = new bool[width * height];
        mask.FillMaskCache(width, height, data);
        Assert.Equal(width * height, data.Length);
        Assert.Equal(bmpData.Select(x => x > 0), data);
    }

    [Fact]
    public unsafe void CreateMaskCacheTest_Mask_Alpha8()
    {
        int width = 8, height = 6;
        using SKBitmap bmp = new(width, height, SKColorType.Alpha8, SKAlphaType.Premul);
        byte* pbmp = (byte*)bmp.GetPixels();
        for (int i = 0; i < 5; ++i)
        {
            pbmp[Random.Shared.Next(width * height)] = (byte)Random.Shared.Next(1, 256);
        }
        MaskOptions mask = MaskOptions.CreateWithAllowedFillColor(bmp, SKColors.Transparent);
        bool[] cache = new bool[width * height];
        mask.FillMaskCache(width, height, cache);

        byte[] bmpData = bmp.GetPixelSpan().ToArray();
        Assert.Equal(width * height, cache.Length);
        Assert.Equal(bmpData.Select(x => x > 0), cache);
    }

    [Fact]
    public unsafe void CreateMaskCacheTest_Mask_8888()
    {
        int width = 8, height = 6;
        using SKBitmap bmp = new(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        byte* pbmp = (byte*)bmp.GetPixels();
        for (int i = 0; i < 5; ++i)
        {
            int pixelId = Random.Shared.Next(width * height);
            pbmp[pixelId * 4] = (byte)Random.Shared.Next(1, 256);
            pbmp[pixelId * 4 + 3] = 255;
        }
        MaskOptions mask = MaskOptions.CreateWithAllowedFillColor(bmp, SKColors.Transparent);

        byte[] bmpData = bmp.GetPixelSpan().ToArray().Chunk(4).Select(x => x[0]).ToArray();
        bool[] cache = new bool[width * height];
        mask.FillMaskCache(width, height, cache);
        Assert.Equal(width * height, cache.Length);
        Assert.Equal(bmpData.Select(x => x > 0), cache);
    }
}