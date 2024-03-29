﻿using SkiaSharp;
using System;
using System.Runtime.CompilerServices;

namespace Sdcb.WordClouds;

internal record IntegralMap(int Width, int Height)
{
    private readonly int[,] _integral = new int[Height, Width];

    public int this[int y, int x] => _integral[y, x];

    public unsafe void Update(bool[,] cache)
    {
        int height = cache.GetLength(0);
        int width = cache.GetLength(1);
        if (Height != height || Width != width)
        {
            throw new ArgumentException("Cache size does not match the integral size.");
        }

        fixed (bool* cachePtr = cache)
        fixed (int* integralPtr = _integral)
        {
            // 初始化第一个元素
            integralPtr[0] = cachePtr[0] ? 1 : 0;

            // 初始化第一行
            for (int x = 1; x < width; x++)
            {
                integralPtr[x] = integralPtr[x - 1] + (cachePtr[x] ? 1 : 0);
            }

            // 初始化第一列
            for (int y = 1; y < height; y++)
            {
                int idx = y * width;
                integralPtr[idx] = integralPtr[idx - width] + (cachePtr[idx] ? 1 : 0);
            }

            // 计算其余的积分图值
            for (int y = 1; y < height; y++)
            {
                for (int x = 1; x < width; x++)
                {
                    int idx = y * width + x;
                    // 当前点的积分值 = 左边的积分值 + 上边的积分值 - 左上角的积分值 + 当前点的值
                    integralPtr[idx] = integralPtr[idx - 1] + integralPtr[idx - width]
                                        - integralPtr[idx - width - 1] + (cachePtr[idx] ? 1 : 0);
                }
            }
        }
    }

    public unsafe void Update(bool[,] cache, int startX, int startY)
    {
        if (startX == 0 && startY == 0)
        {
            Update(cache);
            return;
        }

        int height = cache.GetLength(0);
        int width = cache.GetLength(1);
        if (Height != height || Width != width)
        {
            throw new ArgumentException("Cache size does not match the integral size.");
        }
        if (startX < 0 || startX >= width || startY < 0 || startY >= height)
        {
            throw new ArgumentOutOfRangeException($"Start coordinates ({startX},{startY}) are out of cache bounds.");
        }

        fixed (bool* cachePtr = cache)
        fixed (int* integralPtr = _integral)
        {
            if (startY == 0)
            {
                // 初始化第一行
                for (int x = 1; x < width; x++)
                {
                    integralPtr[x] = integralPtr[x - 1] + (cachePtr[x] ? 1 : 0);
                }
            }
            
            if (startX == 0)
            {
                // 初始化第一列
                for (int y = 1; y < height; y++)
                {
                    int idx = y * width;
                    integralPtr[idx] = integralPtr[idx - width] + (cachePtr[idx] ? 1 : 0);
                }
            }

            // 计算其余的积分图值
            for (int y = Math.Max(1, startY); y < height; y++)
            {
                for (int x = Math.Max(1, startX); x < width; x++)
                {
                    int idx = y * width + x;
                    // 当前点的积分值 = 左边的积分值 + 上边的积分值 - 左上角的积分值 + 当前点的值
                    integralPtr[idx] = integralPtr[idx - 1] + integralPtr[idx - width]
                                        - integralPtr[idx - width - 1] + (cachePtr[idx] ? 1 : 0);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetSum(in SKRectI rect)
    {
        int left = rect.Left;
        int top = rect.Top;
        int right = rect.Right;
        int bottom = rect.Bottom;

        if (left < 0 || top < 0 || right >= Width || bottom >= Height)
        {
            throw new ArgumentOutOfRangeException(nameof(rect));
        }

        int sum = _integral[bottom, right];
        if (left > 0)
        {
            sum -= _integral[bottom, left - 1];
        }
        if (top > 0)
        {
            sum -= _integral[top - 1, right];
        }
        if (left > 0 && top > 0)
        {
            sum += _integral[top - 1, left - 1];
        }
        return sum;
    }

    internal int[] ToArray() => Utils.Convert2DTo1D(_integral);
}

