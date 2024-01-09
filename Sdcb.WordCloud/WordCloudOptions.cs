using SkiaSharp;
using System;
using System.Drawing;

namespace Sdcb.WordClouds;

public record WordCloudOptions
{
    public int Width { get; set; } = 800;

    public int Height { get; set; } = 600;

    private int? _maxFontSize = null;
    public int MaxFontSize { get => _maxFontSize ?? Math.Max(Width, Height); set => _maxFontSize = value; }

    public float FontStep { get; set; } = 1.0f;

    public FontColorAccessor FontColorAccessor { get; set; } = _ => Utils.RandomColor;

    public FontSizeAccessor FontSizeAccessor { get; set; } = ctx => (float)Math.Min(ctx.CurrentFontSize, 100 * Math.Log10(ctx.Frequency + 100));

    public SKBitmap? Background { get; set; }
}

public abstract record WordCloudBackground;
internal record SolidColorWordCloudBackground(SKColor Color)
{
    public static SKColor Transparent { get; }
}

public delegate float FontSizeAccessor(WordCloudContext context);

public delegate SKColor FontColorAccessor(WordCloudContext context);

public record WordCloudContext(string Word, int Frequency, int CurrentFontSize) : WordFrequency(Word, Frequency);