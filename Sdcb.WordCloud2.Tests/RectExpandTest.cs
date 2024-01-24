using SkiaSharp;

namespace Sdcb.WordClouds.Tests;

public class SkiaRectExpansionTests
{
    // 测试水平扩展
    [Theory]
    [InlineData(100, 100, 50, 50)]
    [InlineData(100, 100, 51, 49)] // 奇数宽度
    [InlineData(100, 100, 0, 100)] // 宽度为0
    public void ExpandHorizontally_ReturnsCorrectRectangle(int centerX, int centerY, int width, int height)
    {
        // Arrange
        var center = new SKPointI(centerX, centerY);
        var actualHalfWidth = width / 2;
        var actualOtherHalfWidth = width - actualHalfWidth;
        var actualHalfHeight = height / 2;
        var actualOtherHalfHeight = height - actualHalfHeight;

        // Act
        OrientationRect orp = OrientationRect.ExpandHorizontally(center, new (width, height));

        // Assert
        Assert.Equal(centerX - actualHalfWidth, orp.Rect.Left);
        Assert.Equal(centerY - actualHalfHeight, orp.Rect.Top);
        Assert.Equal(centerX + actualOtherHalfWidth, orp.Rect.Right);
        Assert.Equal(centerY + actualOtherHalfHeight, orp.Rect.Bottom);
    }

    // 测试垂直扩展 - 这次也确保使用all - half的计算方法
    [Theory]
    [InlineData(100, 100, 50, 50)]
    [InlineData(100, 100, 49, 51)] // 奇数高度
    [InlineData(100, 100, 100, 0)] // 高度为0
    public void ExpandVertically_ReturnsCorrectRectangle(int centerX, int centerY, int width, int height)
    {
        // Arrange
        var center = new SKPointI(centerX, centerY);
        var actualHalfWidth = width / 2;
        var actualOtherHalfWidth = width - actualHalfWidth;
        var actualHalfHeight = height / 2;
        var actualOtherHalfHeight = height - actualHalfHeight;

        // Act
        OrientationRect rect = OrientationRect.ExpandVertically(center, new(width, height));

        // Assert
        Assert.Equal(centerX - actualHalfWidth, rect.Rect.Left);
        Assert.Equal(centerY - actualHalfHeight, rect.Rect.Top);
        Assert.Equal(centerX + actualOtherHalfWidth, rect.Rect.Right);
        Assert.Equal(centerY + actualOtherHalfHeight, rect.Rect.Bottom);
    }

    // 测试边界条件
    [Fact]
    public void ExpandHorizontally_ZeroWidth_CreatesLine()
    {
        // Arrange
        var center = new SKPointI(100, 100);
        var width = 0;
        var height = 100;
        var actualHalfHeight = height / 2;
        var actualOtherHalfHeight = height - actualHalfHeight;

        // Act
        OrientationRect rect = OrientationRect.ExpandHorizontally(center, new (width, height));

        // Assert
        Assert.Equal(center.X, rect.Rect.Left);
        Assert.Equal(center.X, rect.Rect.Right); // 这确保了宽度为0时是一条线
        Assert.Equal(center.Y - actualHalfHeight, rect.Rect.Top);
        Assert.Equal(center.Y + actualOtherHalfHeight, rect.Rect.Bottom);
    }

    [Fact]
    public void ExpandVertically_ZeroHeight_CreatesLine()
    {
        // Arrange
        var center = new SKPointI(100, 100);
        var width = 100;
        var height = 0;
        var actualHalfWidth = width / 2;
        var actualOtherHalfWidth = width - actualHalfWidth;

        // Act
        OrientationRect rect = OrientationRect.ExpandVertically(center, new (width, height));

        // Assert
        Assert.Equal(center.Y, rect.Rect.Top);
        Assert.Equal(center.Y, rect.Rect.Bottom); // 这确保了高度为0时是一条线
        Assert.Equal(center.X - actualHalfWidth, rect.Rect.Left);
        Assert.Equal(center.X + actualOtherHalfWidth, rect.Rect.Right);
    }
}