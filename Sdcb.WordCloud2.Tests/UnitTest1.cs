using Sdcb.WordClouds;
using SkiaSharp;

namespace Sdcb.WordCloud2.Tests;

public class UnitTest1
{
    [Fact]
    public void CreateMaskCacheTest_Empty()
    {
        int width = 80, height = 60;
        bool[] data = WordCloud.CreateMaskCache(width, height, null);
        Assert.Equal(width * height, data.Length);
        Assert.Equal(Enumerable.Range(0, width * height).Select(x => false), data);
    }

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
        byte[] bmpData = bmp.GetPixelSpan().ToArray();

        bool[] data = WordCloud.CreateMaskCache(width, height, bmp);
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
        byte[] bmpData = bmp.GetPixelSpan().ToArray();

        bool[] data = WordCloud.CreateMaskCache(width, height, bmp);
        Assert.Equal(width * height, data.Length);
        Assert.Equal(bmpData.Select(x => x > 0), data);
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
        byte[] bmpData = bmp.GetPixelSpan().ToArray().Chunk(4).Select(x => x[0]).ToArray();

        bool[] data = WordCloud.CreateMaskCache(width, height, bmp);
        Assert.Equal(width * height, data.Length);
        Assert.Equal(bmpData.Select(x => x > 0), data);
    }
}