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
        var rect = WordCloudFactory.ExpandHorizontally(center, width, height);

        // Assert
        Assert.Equal(centerX - actualHalfWidth, rect.Left);
        Assert.Equal(centerY - actualHalfHeight, rect.Top);
        Assert.Equal(centerX + actualOtherHalfWidth, rect.Right);
        Assert.Equal(centerY + actualOtherHalfHeight, rect.Bottom);
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
        var rect = WordCloudFactory.ExpandVertically(center, width, height);

        // Assert
        Assert.Equal(centerX - actualHalfWidth, rect.Left);
        Assert.Equal(centerY - actualHalfHeight, rect.Top);
        Assert.Equal(centerX + actualOtherHalfWidth, rect.Right);
        Assert.Equal(centerY + actualOtherHalfHeight, rect.Bottom);
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
        var rect = WordCloudFactory.ExpandHorizontally(center, width, height);

        // Assert
        Assert.Equal(center.X, rect.Left);
        Assert.Equal(center.X, rect.Right); // 这确保了宽度为0时是一条线
        Assert.Equal(center.Y - actualHalfHeight, rect.Top);
        Assert.Equal(center.Y + actualOtherHalfHeight, rect.Bottom);
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
        var rect = WordCloudFactory.ExpandVertically(center, width, height);

        // Assert
        Assert.Equal(center.Y, rect.Top);
        Assert.Equal(center.Y, rect.Bottom); // 这确保了高度为0时是一条线
        Assert.Equal(center.X - actualHalfWidth, rect.Left);
        Assert.Equal(center.X + actualOtherHalfWidth, rect.Right);
    }
}