using Sdcb.WordClouds;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdcb.WordClouds.Tests;

public class PointTraverserTest
{
    [Fact]
    public void StartsAtOrigin_TraversesAllPoints()
    {
        var size = new SKSizeI(2, 2);
        var startPoint = new SKPointI(0, 0);
        var points = WordCloudFactory.TraversePointsSequentially(size, startPoint).ToArray();

        // Checking the total number of points
        Assert.Equal(4, points.Length);

        // Checking each individual point
        Assert.Contains(new SKPointI(0, 0), points);
        Assert.Contains(new SKPointI(1, 0), points);
        Assert.Contains(new SKPointI(1, 1), points);
        Assert.Contains(new SKPointI(0, 1), points);
    }

    [Fact]
    public void StartsAtOrigin_9_TraversesAllPoints()
    {
        SKSizeI size = new(3, 3);
        SKPointI startPoint = new(1, 1);
        SKPointI[] points = WordCloudFactory.TraversePointsSequentially(size, startPoint).ToArray();

        // Checking the total number of points
        Assert.Equal(9, points.Length);
        SKPointI[] expectedPoints = 
        [
            new SKPointI(1, 1), new SKPointI(2, 1), 
            new SKPointI(0, 2), new SKPointI(1, 2), new SKPointI(2, 2), 
            new SKPointI(0, 0), new SKPointI(1, 0), new SKPointI(2, 0), 
            new SKPointI(0, 1)
        ];
        Assert.Equal(expectedPoints, points);
    }

    [Fact]
    public void StartPointOutOfBounds_ReturnsEmptyEnumeration()
    {
        var size = new SKSizeI(3, 3);
        var startPoint = new SKPointI(-1, -1);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var points = WordCloudFactory.TraversePointsSequentially(size, startPoint).ToArray();
        });
    }

    [Fact]
    public void SinglePoint_ReturnsOnlyThatPoint()
    {
        var size = new SKSizeI(1, 1);
        var startPoint = new SKPointI(0, 0);
        var points = WordCloudFactory.TraversePointsSequentially(size, startPoint).ToArray();

        Assert.Single(points);
        Assert.Equal(startPoint, points[0]);
    }

    [Fact]
    public void LargeGrid_TraversesAllPoints()
    {
        var size = new SKSizeI(10, 10);
        var startPoint = new SKPointI(5, 5);
        var points = WordCloudFactory.TraversePointsSequentially(size, startPoint).ToArray();

        Assert.Equal(100, points.Length);
        Assert.All(points, point => Assert.True(point.X >= 0 && point.X < size.Width && point.Y >= 0 && point.Y < size.Height));
    }

    [Fact]
    public void ZeroSizeGrid_ReturnsNoPoints()
    {
        var size = new SKSizeI(0, 0);
        var startPoint = new SKPointI(0, 0);
        var points = WordCloudFactory.TraversePointsSequentially(size, startPoint).ToArray();

        Assert.Empty(points);
    }
}
