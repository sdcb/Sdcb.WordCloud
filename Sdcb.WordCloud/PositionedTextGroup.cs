using SkiaSharp;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace Sdcb.WordClouds;

/// <summary>
/// Represents a group of text segments positioned within a coordinate space.
/// </summary>
/// <param name="Texts">An array of <see cref="PositionedText"/> objects representing individual text segments.</param>
public record PositionedTextGroup(PositionedText[] Texts)
{
    /// <summary>
    /// Gets the width of the text group, defined by the right-most position of the last text segment.
    /// </summary>
    [JsonIgnore]
    public readonly float Width = Texts.Length > 0 ? Texts[^1].Right : 0;

    /// <summary>
    /// Gets the maximum height of the text segments in the text group.
    /// </summary>
    [JsonIgnore]
    public readonly float Height = Texts.Max(x => x.Height);

    /// <summary>
    /// Gets the size of the text group as a two-dimensional vector (width and height).
    /// </summary>
    [JsonIgnore]
    public readonly SKSize Size = new(Texts.Length > 0 ? Texts[^1].Right : 0, Texts.Max(x => x.Height));

    /// <summary>
    /// Gets the size of the text group as an integer two-dimensional vector (width and height),
    /// rounded up to the nearest integer.
    /// </summary>
    [JsonIgnore]
    public readonly SKSizeI SizeI = new((int)Math.Ceiling(Texts.Length > 0 ? Texts[^1].Right : 0), (int)Math.Ceiling(Texts.Max(x => x.Height)));

    /// <summary>
    /// Creates a bitmap layout of the text group using a specified paint.
    /// </summary>
    /// <param name="paint">The <see cref="SKPaint"/> to use for drawing the text segments.</param>
    /// <returns>A <see cref="SKBitmap"/> representing the rendered text segments.</returns>
    public SKBitmap CreateTextLayout(SKPaint paint)
    {
        SKBitmap temp = new(SizeI.Width, SizeI.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        using SKCanvas tempCanvas = new(temp);
        foreach (PositionedText segment in Texts)
        {
            paint.Typeface = segment.Typeface;
            tempCanvas.DrawText(segment.Text, segment.Left, -paint.FontMetrics.Ascent, paint);
        }

        return temp;
    }
}
