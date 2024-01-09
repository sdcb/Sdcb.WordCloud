using SkiaSharp;
using System;

namespace Sdcb.WordClouds;

internal class Utils
{
    static readonly Random _r = new();

    public static SKColor RandomColor => new SKColor((byte)_r.Next(0, 256), (byte)_r.Next(0, 256), (byte)_r.Next(0, 256));
}
