using SkiaSharp;
using System.Text.Json.Serialization;

namespace Sdcb.WordClouds;

/// <summary>
/// Represents the positioned text used in the word cloud.
/// </summary>
public record PositionedText
{
    /// <summary>
    /// The text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// The typeface of the text.
    /// </summary>
    [JsonConverter(typeof(SKTypefaceConverter))]
    public SKTypeface Typeface { get; }

    /// <summary>
    /// The width of the text.
    /// </summary>
    public float Width { get; }

    /// <summary>
    /// The left position of the text.
    /// </summary>
    public float Left { get; }

    /// <summary>
    /// The height of the text.
    /// </summary>
    public float Height { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionedText"/> class.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="typeface">The typeface of the text.</param>
    /// <param name="width">The width of the text.</param>
    /// <param name="left">The left position of the text.</param>
    /// <param name="height">The height of the text.</param>
    public PositionedText(string text, SKTypeface typeface, float width, float left, float height)
    {
        Text = text;
        Typeface = typeface;
        Width = width;
        Left = left;
        Height = height;
        Right = Left + Width;
    }

    /// <summary>
    /// Gets the right position of the text.
    /// </summary>
    [JsonIgnore]
    public float Right { get; }
}
