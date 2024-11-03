using SkiaSharp;
using System;
using System.Runtime.CompilerServices;

namespace Sdcb.WordClouds;

internal static class TraverserHelper
{
    public static Traverser CreateTraverser(TextOrientations orientations, Random random)
    {
        return orientations switch
        {
            TextOrientations.HorizontalOnly => HorizontalOnly,
            TextOrientations.VerticalOnly => VerticalOnly,
            TextOrientations.PreferHorizontal => PreferHorizontal,
            TextOrientations.PreferVertical => PreferVertical,
            TextOrientations.Random => (p, r, i) => random.Next(2) == 0 ? HorizontalOnly(p, r, i) : VerticalOnly(p, r, i),
            _ => throw new NotImplementedException(),
        };
    }

    private static OrientationRect? PreferHorizontal(SKPointI startPoint, SKSizeI rectSize, IntegralMap integralMap)
    {
        FastCommonCheck(startPoint, integralMap);

        // Begin traversal at startPoint
        int cx = startPoint.X, cy = startPoint.Y;
        int width = integralMap.Width;
        int height = integralMap.Height;

        // Continue indefinitely until we loop back to the start point
        do
        {
            // Yield the current point
            {
                OrientationRect orp = OrientationRect.ExpandHorizontally(new SKPointI(cx, cy), rectSize);
                if (orp.IsInside(width, height) && integralMap.GetSum(orp.Rect) <= 0)
                {
                    return orp;
                }
            }
            {
                OrientationRect orp = OrientationRect.ExpandVertically(new SKPointI(cx, cy), rectSize);
                if (orp.IsInside(width, height) && integralMap.GetSum(orp.Rect) <= 0)
                {
                    return orp;
                }
            }

            // Move to the next point
            cx++;

            // If we reach the end of the row, move to the next row
            if (cx >= width)
            {
                cx = 0;
                cy++;
            }

            // If we reach the end of the columns, start back at the top
            if (cy >= height)
            {
                cy = 0;
            }

            // If after wrapping around we're at the start point, stop traversing
        } while (cx != startPoint.X || cy != startPoint.Y);

        return null;
    }

    private static OrientationRect? PreferVertical(SKPointI startPoint, SKSizeI rectSize, IntegralMap integralMap)
    {
        FastCommonCheck(startPoint, integralMap);

        // Begin traversal at startPoint
        int cx = startPoint.X, cy = startPoint.Y;
        int width = integralMap.Width;
        int height = integralMap.Height;

        // Continue indefinitely until we loop back to the start point
        do
        {
            // Yield the current point
            {
                OrientationRect orp = OrientationRect.ExpandVertically(new SKPointI(cx, cy), rectSize);
                if (orp.IsInside(width, height) && integralMap.GetSum(orp.Rect) <= 0)
                {
                    return orp;
                }
            }
            {
                OrientationRect orp = OrientationRect.ExpandHorizontally(new SKPointI(cx, cy), rectSize);
                if (orp.IsInside(width, height) && integralMap.GetSum(orp.Rect) <= 0)
                {
                    return orp;
                }
            }

            // Move to the next point in vertical direction first
            cy++;

            // If we reach the end of the column, move to the next column
            if (cy >= height)
            {
                cy = 0;
                cx++;
            }

            // If we reach the end of the rows, start back at the left
            if (cx >= width)
            {
                cx = 0;
            }

            // If after wrapping around we're at the start point, stop traversing
        } while (cx != startPoint.X || cy != startPoint.Y);

        return null;
    }

    private static OrientationRect? HorizontalOnly(SKPointI startPoint, SKSizeI rectSize, IntegralMap integralMap)
    {
        FastCommonCheck(startPoint, integralMap);

        // Begin traversal at startPoint
        int cx = startPoint.X, cy = startPoint.Y;
        int width = integralMap.Width;
        int height = integralMap.Height;

        // Continue indefinitely until we loop back to the start point
        do
        {
            // Yield the current point
            OrientationRect orp = OrientationRect.ExpandHorizontally(new SKPointI(cx, cy), rectSize);
            if (orp.IsInside(width, height) && integralMap.GetSum(orp.Rect) <= 0)
            {
                return orp;
            }

            // Move to the next point
            cx++;

            // If we reach the end of the row, move to the next row
            if (cx >= width)
            {
                cx = 0;
                cy++;
            }

            // If we reach the end of the columns, start back at the top
            if (cy >= height)
            {
                cy = 0;
            }

            // If after wrapping around we're at the start point, stop traversing
        } while (cx != startPoint.X || cy != startPoint.Y);

        return null;
    }

    private static OrientationRect? VerticalOnly(SKPointI startPoint, SKSizeI rectSize, IntegralMap integralMap)
    {
        FastCommonCheck(startPoint, integralMap);

        // Begin traversal at startPoint
        int cx = startPoint.X, cy = startPoint.Y;
        int width = integralMap.Width;
        int height = integralMap.Height;

        // Continue indefinitely until we loop back to the start point
        do
        {
            // Yield the current point
            OrientationRect orp = OrientationRect.ExpandVertically(new SKPointI(cx, cy), rectSize);
            if (orp.IsInside(width, height) && integralMap.GetSum(orp.Rect) <= 0)
            {
                return orp;
            }

            // Move to the next point in vertical direction first
            cy++;

            // If we reach the end of the column, move to the next column
            if (cy >= height)
            {
                cy = 0;
                cx++;
            }

            // If we reach the end of the rows, start back at the left
            if (cx >= width)
            {
                cx = 0;
            }

            // If after wrapping around we're at the start point, stop traversing
        } while (cx != startPoint.X || cy != startPoint.Y);

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FastCommonCheck(SKPointI startPoint, IntegralMap integralMap)
    {
        // Ensure the start point is within bounds
        if (startPoint.X < 0 || startPoint.X >= integralMap.Width ||
            startPoint.Y < 0 || startPoint.Y >= integralMap.Height)
        {
            throw new ArgumentOutOfRangeException(nameof(startPoint));
        }
    }
}

internal delegate OrientationRect? Traverser(
        SKPointI startPoint,
        SKSizeI rectSize,
        IntegralMap integralMap);