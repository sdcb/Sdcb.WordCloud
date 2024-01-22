using Sdcb.WordClouds;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdcb.WordCloud2.Tests;

public class FillCacheTest
{
    [Fact]
    public unsafe void Simple()
    {
        // arrange
        int width = 3, height = 2;
        using SKBitmap bmp = new(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        // draw seconds line to white
        uint* pbmp = (uint*)bmp.GetPixels();
        pbmp[width * 1 + 0] = 0xffffffff;
        pbmp[width * 1 + 1] = 0xffffffff;
        pbmp[width * 1 + 2] = 0xffffffff;
        int cacheWidth = 4, cacheHeight = 5;
        bool[,] cache = new bool[cacheHeight, cacheWidth];
        SKRectI rect = SKRectI.Create(1, 1, width, height);

        // act
        WordCloudFactory.FillCache(bmp, rect, TextOrientations.Horizontal, cache);

        // assert
        Assert.Equal(
        [ 
            false, false, false, false,
            false, false, false, false,
            false, true, true, true,
            false, false, false, false,
            false, false, false, false
        ], Utils.Convert2DTo1D(cache));
    }
}
