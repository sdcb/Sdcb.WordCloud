using SkiaSharp;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace Sdcb.WordClouds;

public record PositionedTextGroup(PositionedText[] Texts)
{
    [JsonIgnore]
    public readonly float Width = Texts.Length > 0 ? Texts[^1].Right : 0;

    [JsonIgnore]
    public readonly float Height = Texts.Max(x => x.Height);

    [JsonIgnore]
    public readonly SKSize Size = new(Texts.Length > 0 ? Texts[^1].Right : 0, Texts.Max(x => x.Height));

    [JsonIgnore]
    public readonly SKSizeI SizeI = new((int)Math.Ceiling(Texts.Length > 0 ? Texts[^1].Right : 0), (int)Math.Ceiling(Texts.Max(x => x.Height)));

    public SKBitmap CreateTextLayout(SKPaint paint)
    {
        SKBitmap temp = new(SizeI.Width, SizeI.Height);
        using SKCanvas tempCanvas = new(temp);
        foreach (PositionedText segment in Texts)
        {
            paint.Typeface = segment.Typeface;
            tempCanvas.DrawText(segment.Text, segment.Left, -paint.FontMetrics.Ascent, paint);
        }

        return temp;
    }
}
