using SkiaSharp;
using System.Runtime.CompilerServices;

namespace Sdcb.WordClouds;

internal record struct OrientationRect(TextOrientations Orientations, SKRectI Rect, SKPointI Center)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static OrientationRect ExpandHorizontally(SKPointI center, SKSizeI size)
    {
        int halfWidth = size.Width / 2;
        int otherHalfWidth = size.Width - halfWidth;
        int halfHeight = size.Height / 2;
        int otherHalfHeight = size.Height - halfHeight;
        // 宽度在水平方向上分布，因此需要调整中心点的X坐标。
        // 高度在垂直方向上均匀分布，因此中心点的Y坐标上下均匀分布halfHeight。
        return new (TextOrientations.Horizontal, new SKRectI(center.X - halfWidth, center.Y - halfHeight, center.X + otherHalfWidth, center.Y + otherHalfHeight), center);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static OrientationRect ExpandVertically(SKPointI center, SKSizeI size)
    {
        int halfWidth = size.Width / 2;
        int otherHalfWidth = size.Width - halfWidth;
        int halfHeight = size.Height / 2;
        int otherHalfHeight = size.Height - halfHeight;
        // 高度在垂直方向上分布，因此需要调整中心点的Y坐标。
        // 宽度在水平方向上均匀分布，因此中心点的X坐标左右均匀分布halfWidth。
        return new(TextOrientations.Vertical, new SKRectI(center.X - halfHeight, center.Y - halfWidth, center.X + otherHalfHeight, center.Y + otherHalfWidth), center);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInside(int width, int height)
    {
        return Rect.Right < width && Rect.Bottom < height && Rect.Left >= 0 && Rect.Top >= 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float ToDegree()
    {
#pragma warning disable CS8524 // switch 表达式不会处理其输入类型的某些值(它不是穷举)，这包括未命名的枚举值。
        return Orientations switch
        {
            TextOrientations.Horizontal => 0,
            TextOrientations.Vertical => 90,
        };
    }
}