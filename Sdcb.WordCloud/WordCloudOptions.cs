using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Sdcb.WordClouds;

/// <summary>
/// Represents configuration options for a word cloud.
/// </summary>
/// <param name="Width">The width of the word cloud in pixels.</param>
/// <param name="Height">The height of the word cloud in pixels.</param>
/// <param name="WordScores">An collection of <see cref="WordScores"/> to determine the word and its corresponding score in the word cloud.</param>
public record WordCloudOptions(int Width, int Height, IEnumerable<WordScore> WordScores)
{
    /// <summary>
    /// Stores the size of the word cloud as a width-height pair.
    /// </summary>
    public readonly SKSizeI Size = new(Width, Height);

    /// <summary>
    /// The initial font size for the words in the word cloud.
    /// </summary>
    public float? InitialFontSize { get; set; }

    /// <summary>
    /// Gets the initial font size for the words or the minimum dimension of the word cloud if not set.
    /// </summary>
    /// <returns>The default or defined initial font size for the words in the word cloud.</returns>
    public float GetInitialFontSize() => InitialFontSize ?? Math.Min(Width, Height);

    /// <summary>
    /// The step to reduce the font size on each iteration when trying to fit the word onto the canvas.
    /// </summary>
    public float FontStep { get; set; } = 1.0f;

    /// <summary>
    /// The minimum font size a word can have.
    /// </summary>
    public float MinFontSize { get; set; } = 4.0f;

    /// <summary>
    /// Delegate to determine the color of a word based on the context it's in.
    /// </summary>
    public FontColorAccessor FontColorAccessor { get; set; } = ctx => new((byte)ctx.Random.Next(0, 256), (byte)ctx.Random.Next(0, 256), (byte)ctx.Random.Next(0, 256));

    /// <summary>
    /// Delegate to determine the font size of a word based on the context it's in.
    /// </summary>
    public FontSizeAccessor FontSizeAccessor { get; set; } = ctx => (float)Math.Min(ctx.CurrentFontSize, 100 * Math.Log10(ctx.Frequency + 100));

    /// <summary>
    /// An optional background image for the word cloud.
    /// </summary>
    public SKBitmap? Background { get; set; }

    /// <summary>
    /// Optional mask options to apply during the rendering of the word cloud.
    /// </summary>
    public MaskOptions? Mask { get; set; }

    /// <summary>
    /// Manages fonts for the word cloud.
    /// </summary>
    public FontManager FontManager { get; set; } = new();

    /// <summary>
    /// Random number generator used in various operations within word cloud generation.
    /// </summary>
    public Random Random { get; set; } = new();

    /// <summary>
    /// Gets a random starting point within the bounds of the word cloud for placing a word.
    /// </summary>
    /// <returns>A point represented by <see cref="SKPointI"/> within the word cloud dimensions.</returns>
    public virtual SKPointI GetRandomStartPoint()
    {
        return new(Random.Next(0, Width), Random.Next(0, Height));
    }

    /// <summary>
    /// The orientations in which text can be placed in the word cloud.
    /// </summary>
    public TextOrientations TextOrientation { get; set; } = TextOrientations.PreferHorizontal;
}

/// <summary>
/// Delegate that defines the signature for accessing font size in a word cloud context.
/// </summary>
/// <param name="context">The context containing information about the word for font size determination.</param>
/// <returns>The font size to be applied to the word.</returns>
public delegate float FontSizeAccessor(WordCloudContext context);

/// <summary>
/// Delegate that defines the signature for accessing font color in a word cloud context.
/// </summary>
/// <param name="context">The context containing information about the word for color determination.</param>
/// <returns>The <see cref="SKColor"/> to be applied to the word.</returns>
public delegate SKColor FontColorAccessor(WordCloudContext context);

/// <summary>
/// Provides contextual information for the word being processed in the word cloud, inheriting from <see cref="WordScore"/>.
/// </summary>
/// <param name="Random">An instance of <see cref="System.Random"/> used for randomness.</param>
/// <param name="Word">The word being processed.</param>
/// <param name="Frequency">The frequency of the word which may influence font size or other characteristics.</param>
/// <param name="CurrentFontSize">The current computed font size for the word.</param>
public record WordCloudContext(Random Random, string Word, int Frequency, float CurrentFontSize) : WordScore(Word, Frequency);

internal enum HorizontalOrVertical
{
    Horizontal,
    Vertical
}

/// <summary>
/// Defines possible text orientations in a word cloud.
/// </summary>
[Flags]
public enum TextOrientations
{
    /// <summary>
    /// Represents a preference for horizontal orientation but allows for vertical as well.
    /// </summary>
    PreferHorizontal,

    /// <summary>
    /// Represents a preference for vertical orientation but allows for horizontal as well.
    /// </summary>
    PreferVertical,

    /// <summary>
    /// Represents horizontal orientation of text.
    /// </summary>
    HorizontalOnly,

    /// <summary>
    /// Represents vertical orientation of text.
    /// </summary>
    VerticalOnly,

    /// <summary>
    /// Represents a random orientation of text, potentially varying from one word to the next.
    /// </summary>
    /// <remarks>
    /// Due to scanning in only one direction (horizontal or vertical) for each word, it may offer a speed advantage.
    /// </remarks>
    Random
}
