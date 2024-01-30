using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sdcb.WordClouds;

public record WordCloudOptions(int Width, int Height, IEnumerable<WordScore> WordFrequencies)
{
    public readonly SKSizeI Size = new(Width, Height);

    public float? InitialFontSize { get; set; }

    public float GetInitialFontSize() => InitialFontSize ?? Math.Min(Width, Height);

    public float FontStep { get; set; } = 1.0f;

    public float MinFontSize { get; set; } = 4.0f;

    public FontColorAccessor FontColorAccessor { get; set; } = ctx => new((byte)ctx.Random.Next(0, 256), (byte)ctx.Random.Next(0, 256), (byte)ctx.Random.Next(0, 256));

    public FontSizeAccessor FontSizeAccessor { get; set; } = ctx => (float)Math.Min(ctx.CurrentFontSize, 100 * Math.Log10(ctx.Frequency + 100));

    public SKBitmap? Background { get; set; }

    public MaskOptions? Mask { get; set; }

    public FontManager FontManager { get; set; } = new();

    public Random Random { get; set; } = new();

    public virtual SKPointI GetRandomStartPoint()
    {
        return new(Random.Next(0, Width), Random.Next(0, Height));
    }

    public TextOrientations TextOrientation { get; set; } = TextOrientations.Horizontal | TextOrientations.Vertical;
}

public delegate float FontSizeAccessor(WordCloudContext context);

public delegate SKColor FontColorAccessor(WordCloudContext context);

public record WordCloudContext(Random Random, string Word, int Frequency, float CurrentFontSize) : WordScore(Word, Frequency);

[Flags]
public enum TextOrientations
{
    Horizontal = 0x01,
    Vertical = 0x10,
}