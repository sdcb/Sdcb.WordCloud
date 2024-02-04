using Sdcb.WordClouds;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sdcb.WordClouds.Tests;

public class FillCacheTest
{
    private readonly ITestOutputHelper _console;

    public FillCacheTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public unsafe void HorizontalTest()
    {
        // arrange
        int width = 3, height = 2;
        using SKBitmap bmp = new(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        {
            // draw seconds line to white
            uint* pbmp = (uint*)bmp.GetPixels();
            pbmp[width * 0 + 1] = 0xffffffff;
            pbmp[width * 1 + 0] = 0xffffffff;
            pbmp[width * 1 + 1] = 0xffffffff;
            pbmp[width * 1 + 2] = 0xffffffff;
        }

        int cacheWidth = 4, cacheHeight = 5;
        bool[,] cache = new bool[cacheHeight, cacheWidth];
        SKRectI rect = SKRectI.Create(1, 1, width, height);

        // act
        WordCloud.FillCache(bmp, TextOrientations.Horizontal, cache, rect);

        // assert
        _console.WriteLine("SKBitmap:");
        WriteBmp(bmp);
        _console.WriteLine("cache:");
        WriteCache(cache);
        CheckCacheAsX(
        [
            "....",
            "..X.",
            ".XXX",
            "....",
            "....",
        ], cache);
    }

    [Fact]
    public unsafe void VerticleTest()
    {
        // arrange
        int width = 3, height = 2;
        using SKBitmap bmp = new(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        {
            // draw seconds line to white
            uint* pbmp = (uint*)bmp.GetPixels();
            pbmp[width * 0 + 1] = 0xffffffff;
            pbmp[width * 1 + 0] = 0xffffffff;
            pbmp[width * 1 + 1] = 0xffffffff;
            pbmp[width * 1 + 2] = 0xffffffff;
        }

        int cacheWidth = 4, cacheHeight = 5;
        bool[,] cache = new bool[cacheHeight, cacheWidth];
        SKRectI rect = SKRectI.Create(1, 1, height, width);

        // act
        WordCloud.FillCache(bmp, TextOrientations.Vertical, cache, rect);

        // assert
        _console.WriteLine("SKBitmap:");
        WriteBmp(bmp);
        _console.WriteLine("cache:");
        WriteCache(cache);
        CheckCacheAsX(
        [
            "....",
            ".X..",
            ".XX.",
            ".X..",
            "....",
        ], cache);
    }

    [Fact]
    public unsafe void BitmapNotBgra8888_ThrowsArgumentException()
    {
        // arrange
        int width = 3, height = 2;
        using SKBitmap bmp = new(width, height, SKColorType.Rgba8888, SKAlphaType.Premul); // 非Bgra8888颜色类型
        bool[,] cache = new bool[height, width];
        SKRectI rect = SKRectI.Create(0, 0, width, height);

        // act & assert
        var ex = Assert.Throws<ArgumentException>(() => WordCloud.FillCache(bmp, TextOrientations.Horizontal, cache, rect));
        Assert.Contains("Bitmap must be of type Bgra8888", ex.Message);
    }

    [Fact]
    public unsafe void CacheTooSmall_ThrowsArgumentException()
    {
        // arrange
        int width = 3, height = 2;
        using SKBitmap bmp = new(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        bool[,] cache = new bool[height, width - 1]; // 缓存数组太小
        SKRectI rect = SKRectI.Create(0, 0, width, height);

        // act & assert
        var ex = Assert.Throws<ArgumentException>(() => WordCloud.FillCache(bmp, TextOrientations.Horizontal, cache, rect));
        Assert.Contains("The cache array is not big enough", ex.Message);
    }

    [Fact]
    public unsafe void CacheCornersTest()
    {
        // arrange
        int width = 3, height = 2;
        using SKBitmap bmp = new(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        {
            // draw seconds line to white
            uint* pbmp = (uint*)bmp.GetPixels();
            pbmp[width * 0 + 1] = 0xffffffff;
            pbmp[width * 1 + 0] = 0xffffffff;
            pbmp[width * 1 + 1] = 0xffffffff;
            pbmp[width * 1 + 2] = 0xffffffff;
        }
        int cacheWidth = 7, cacheHeight = 6;
        bool[,] cache = new bool[cacheHeight, cacheWidth];
        SKRectI topLeft = SKRectI.Create(0, 0, width, height);
        SKRectI topRight = SKRectI.Create(4, 0, width, height);
        SKRectI bottomLeft = SKRectI.Create(1, 3, height, width);
        SKRectI bottomRight = SKRectI.Create(5, 3, height, width);

        // act
        WordCloud.FillCache(bmp, TextOrientations.Horizontal, cache, topLeft);
        WordCloud.FillCache(bmp, TextOrientations.Horizontal, cache, topRight);
        WordCloud.FillCache(bmp, TextOrientations.Vertical, cache, bottomLeft);
        WordCloud.FillCache(bmp, TextOrientations.Vertical, cache, bottomRight);

        // assert
        _console.WriteLine("SKBitmap:");
        WriteBmp(bmp);
        _console.WriteLine("cache:");
        WriteCache(cache);
        CheckCacheAsX(
        [ 
            ".X...X.",
            "XXX.XXX",
            ".......",
            ".X...X.",
            ".XX..XX",
            ".X...X.",
        ], cache);
    }

    [Fact]
    public unsafe void CheckCovers()
    {
        // arrange
        int width = 3, height = 2;
        using SKBitmap bmp = new(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        {
            // draw seconds line to white
            uint* pbmp = (uint*)bmp.GetPixels();
            pbmp[width * 0 + 1] = 0xffffffff;
            pbmp[width * 1 + 0] = 0xffffffff;
            pbmp[width * 1 + 1] = 0xffffffff;
            pbmp[width * 1 + 2] = 0xffffffff;
        }
        int cacheWidth = 3, cacheHeight = 4;
        bool[,] cache = new bool[cacheHeight, cacheWidth];
        SKRectI r1 = SKRectI.Create(0, 0, width, height);
        SKRectI r2 = SKRectI.Create(0, 1, height, width);

        // act
        WordCloud.FillCache(bmp, TextOrientations.Horizontal, cache, r1);
        WordCloud.FillCache(bmp, TextOrientations.Vertical, cache, r2);

        // assert
        _console.WriteLine("SKBitmap:");
        WriteBmp(bmp);
        _console.WriteLine("cache:");
        WriteCache(cache);
        CheckCacheAsX(
        [
            ".X.",
            "XXX",
            "XX.",
            "X..",
        ], cache);
    }

    private void CheckCacheAsX(string[] rows, bool[,] cache)
    {
        string[] cacheStr = cache
            .Cast<bool>()
            .Select(x => x ? 'X' : '.')
            .Chunk(cache.GetLength(1))
            .Select(x => new string(x))
            .ToArray();
        for (int y = 0; y < rows.Length; y++)
        {
            Assert.Equal(rows[y], cacheStr[y]);
        }
    }

    unsafe void WriteBmp(SKBitmap bmp)
    {
        uint* pbmp = (uint*)bmp.GetPixels();
        StringBuilder sb = new();
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                sb.Append(pbmp[y * bmp.Width + x] == 0 ? '.' : 'X');
            }
            sb.AppendLine();
        }
        _console.WriteLine(sb.ToString());
    }

    void WriteCache(bool[,] cache)
    {
        StringBuilder sb = new();
        for (int y = 0; y < cache.GetLength(0); y++)
        {
            for (int x = 0; x < cache.GetLength(1); x++)
            {
                sb.Append(cache[y, x] ? 'X' : '.');
            }
            sb.AppendLine();
        }
        _console.WriteLine(sb.ToString());
    }
}
