using SkiaSharp;
using System.Text.Json.Serialization;

namespace Sdcb.WordClouds;

/// <summary>
/// Represents a line of text, positioned and styled with specific properties.
/// </summary>
public record TextLine
{
    /// <summary>
    /// Gets the group of positioned text elements representing the text line.
    /// </summary>
    public PositionedTextGroup TextGroup { get; }

    /// <summary>
    /// Gets the font size used for displaying the text line.
    /// </summary>
    public float FontSize { get; }

    /// <summary>
    /// Gets the color used to display the text line.
    /// </summary>
    [JsonConverter(typeof(SKColorJsonConverter))]
    public SKColor Color { get; }

    /// <summary>
    /// Gets the center point for the text line, used for positioning it.
    /// </summary>
    [JsonConverter(typeof(SKPointJsonConverter))]
    public SKPoint Center { get; }

    /// <summary>
    /// Gets the rotation angle of the text line in degrees.
    /// </summary>
    public float Rotate { get; }

    /// <summary>
    /// Initializes a new instance of the TextLine class with specified properties.
    /// </summary>
    /// <param name="textGroup">A group of positioned text elements representing the text line.</param>
    /// <param name="fontSize">The size of the font used for the text line.</param>
    /// <param name="color">The color of the text.</param>
    /// <param name="center">The center point for positioning the text line.</param>
    /// <param name="rotate">The rotation angle of the text line in degrees.</param>
    public TextLine(PositionedTextGroup textGroup, float fontSize, SKColor color, SKPoint center, float rotate)
    {
        TextGroup = textGroup;
        FontSize = fontSize;
        Color = color;
        Center = center;
        Rotate = rotate;
    }
}
