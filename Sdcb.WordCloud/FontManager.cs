using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdcb.WordClouds;

/// <summary>
/// Manages the fonts used in the word cloud.
/// </summary>
public class FontManager : IDisposable
{
    /// <summary>
    /// Gets the array of SKTypeface fonts.
    /// </summary>
    public SKTypeface[] Fonts { get; }

    public bool IsOwned { get; }

    public bool Disposed { get; private set; } = false;

    private readonly Dictionary<int, SKTypeface> _mapping = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FontManager"/> class.
    /// </summary>
    public FontManager()
    {
        Fonts = Array.Empty<SKTypeface>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontManager"/> class with the specified fonts.
    /// </summary>
    /// <remarks><see cref="FontManager"/> will take the ownership of <see cref="SKTypeface"/> and in charge of dispose.</remarks>
    /// <param name="fonts">The SKTypeface fonts.</param>
    public FontManager(SKTypeface[] fonts, bool isOwned = true)
    {
        Fonts = fonts;
        IsOwned = isOwned;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontManager"/> class with the specified font family names.
    /// </summary>
    /// <param name="fontFamilyNames">The font family names.</param>
    public FontManager(params string[] fontFamilyNames)
    {
        Fonts = fontFamilyNames.Select(SKTypeface.FromFamilyName).ToArray();
        IsOwned = true;
    }

    /// <summary>
    /// Matches the font for the specified codepoint.
    /// </summary>
    /// <param name="codepoint">The codepoint.</param>
    /// <returns>The matched SKTypeface font.</returns>
    public SKTypeface MatchFont(int codepoint)
    {
        if (_mapping.TryGetValue(codepoint, out SKTypeface? val))
        {
            return val;
        }

        foreach (SKTypeface font in Fonts)
        {
            if (font.ContainsGlyph(codepoint))
            {
                _mapping[codepoint] = font;
                return font;
            }
        }

        SKTypeface final = SKFontManager.Default.MatchCharacter(codepoint);
        _mapping[codepoint] = final;
        return final;
    }

    /// <summary>
    /// Groups the text into single-line segments with corresponding fonts.
    /// </summary>
    /// <param name="fullText">The full text.</param>
    /// <returns>An enumerable of <see cref="TextAndFont"/> representing the grouped text and fonts.</returns>
    public virtual IEnumerable<TextAndFont> GroupTextSingleLine(string fullText)
    {
        SKTypeface lastfont = null!;
        StringBuilder accString = new(capacity: fullText.Length);
        foreach ((int codepoint, string text) in UnicodeCharacterSplit(fullText)
            .Select(x => (codepoint: x, text: Char.ConvertFromUtf32(x))))
        {
            SKTypeface font = MatchFont(codepoint);
            if (lastfont != font)
            {
                if (accString.Length > 0)
                {
                    yield return new TextAndFont(accString.ToString(), lastfont);
                    accString.Clear();
                }
                lastfont = font;
            }
            accString.Append(text);
        }

        if (accString.Length > 0)
        {
            yield return new TextAndFont(accString.ToString(), lastfont);
        }
    }

    /// <summary>
    /// Groups the text into single-line segments with corresponding fonts and positions.
    /// </summary>
    /// <param name="fullText">The full text.</param>
    /// <param name="paint">The SKPaint object used for measuring text.</param>
    /// <returns>An enumerable of <see cref="PositionedText"/> representing the grouped text, fonts, and positions.</returns>
    public virtual IEnumerable<PositionedText> GroupTextSingleLinePositioned(string fullText, SKPaint paint)
    {
        float left = 0;
        foreach (TextAndFont item in GroupTextSingleLine(fullText))
        {
            paint.Typeface = item.Typeface;
            float width = paint.MeasureText(item.Text);
            SKFontMetrics metrics = paint.FontMetrics;
            float height = metrics.Descent - metrics.Ascent;
            yield return new PositionedText(item.Text, item.Typeface, width, left, height);
            left += width;
        }
    }

    /// <summary>
    /// Groups the characters of the text with corresponding fonts.
    /// </summary>
    /// <param name="fullText">The full text.</param>
    /// <returns>An enumerable of <see cref="TextAndFont"/> representing the grouped characters and fonts.</returns>
    public virtual IEnumerable<TextAndFont> GroupCharacters(string fullText)
    {
        return UnicodeCharacterSplit(fullText)
            .Select(x => new TextAndFont(Char.ConvertFromUtf32(x), MatchFont(x)));
    }

    /// <summary>
    /// Splits the input string into Unicode characters.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>An enumerable of Unicode codepoints.</returns>
    public static IEnumerable<int> UnicodeCharacterSplit(string input)
    {
        for (var i = 0; i < input.Length; ++i)
        {
            if (char.IsHighSurrogate(input[i]))
            {
                int length = 0;
                while (true)
                {
                    length += 2;
                    if (i + length < input.Length && input[i + length] == 0x200D)
                    {
                        length += 1;
                    }
                    else
                    {
                        break;
                    }
                }
                yield return Char.ConvertToUtf32(input, i);
                i += length - 1;
            }
            else
            {
                yield return input[i];
            }
        }
    }

    /// <summary>
    /// Creates a text layout bitmap for the specified text using the given paint.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="paint">The SKPaint object used for drawing text.</param>
    /// <returns>The created SKBitmap object representing the text layout.</returns>
    public virtual SKBitmap CreateTextLayout(string text, SKPaint paint)
    {
        PositionedTextGroup group = new(GroupTextSingleLinePositioned(text, paint).ToArray());
        return group.CreateTextLayout(paint);
    }

    /// <summary>
    /// Disposes the FontManager and releases the resources associated with the SKTypeface fonts.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (Disposed) return;

        if (disposing)
        {
            _mapping.Clear();
        }

        if (IsOwned)
        {
            foreach (SKTypeface font in Fonts)
            {
                font.Dispose();
            }
        }
        Disposed = true;
    }

    ~FontManager()
    {
        Dispose(false);
    }
}

/// <summary>
/// Represents the text and font used in the word cloud.
/// </summary>
public record TextAndFont(string Text, SKTypeface Typeface);

/// <summary>
/// Represents the positioned text used in the word cloud.
/// </summary>
public record PositionedText(string Text, SKTypeface Typeface, float Width, float Left, float Height) : TextAndFont(Text, Typeface)
{
    /// <summary>
    /// Gets the right position of the text.
    /// </summary>
    public float Right { get; } = Left + Width;
}

public record PositionedTextGroup(PositionedText[] Texts)
{
    public readonly float Width = Texts.Length > 0 ? Texts[^1].Right : 0;

    public readonly float Height = Texts.Max(x => x.Height);

    public readonly SKSize Size = new(Texts.Length > 0 ? Texts[^1].Right : 0, Texts.Max(x => x.Height));

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