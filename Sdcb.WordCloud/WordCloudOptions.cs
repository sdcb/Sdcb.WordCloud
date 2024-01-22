using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Sdcb.WordClouds;

public record WordCloudOptions(IEnumerable<WordFrequency> WordFrequencies)
{
    public int Width { get; set; } = 800;

    public int Height { get; set; } = 600;

    public float? InitialFontSize { get; set; }

    public float GetInitialFontSize() => InitialFontSize ?? Math.Min(Width, Height);

    public float FontStep { get; set; } = 1.0f;

    public FontColorAccessor FontColorAccessor { get; set; } = _ => Utils.RandomColor;

    public FontSizeAccessor FontSizeAccessor { get; set; } = ctx => (float)Math.Min(ctx.CurrentFontSize, 100 * Math.Log10(ctx.Frequency + 100));

    public SKBitmap? Background { get; set; }

    public MaskOptions? Mask { get; set; }

    public FontManager FontManager { get; set; } = new();
}

public delegate float FontSizeAccessor(WordCloudContext context);

public delegate SKColor FontColorAccessor(WordCloudContext context);

public record WordCloudContext(string Word, int Frequency, float CurrentFontSize) : WordFrequency(Word, Frequency);