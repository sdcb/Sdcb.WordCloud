using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdcb.WordClouds;

internal class FontManager : IDisposable
{
    public SKTypeface[] Fonts { get; }
    public Dictionary<int, SKTypeface> Mapping { get; } = new();

    public FontManager(params SKTypeface[] fonts)
    {
        Fonts = fonts;
    }

    public FontManager(params string[] fontFamilyNames)
    {
        Fonts = fontFamilyNames.Select(SKTypeface.FromFamilyName).ToArray();
    }

    public SKTypeface MatchFont(int codepoint)
    {
        if (Mapping.TryGetValue(codepoint, out SKTypeface? val))
        {
            return val;
        }

        foreach (SKTypeface font in Fonts)
        {
            if (font.ContainsGlyph(codepoint))
            {
                Mapping[codepoint] = font;
                return font;
            }
        }

        SKTypeface final = SKFontManager.Default.MatchCharacter(codepoint);
        Mapping[codepoint] = final;
        return final;
    }

    public IEnumerable<TextAndFont> GroupTextSingleLine(string fullText)
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

    public IEnumerable<TextAndFont> GroupCharacters(string fullText)
    {
        return UnicodeCharacterSplit(fullText)
            .Select(x => new TextAndFont(Char.ConvertFromUtf32(x), MatchFont(x)));
    }

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

    public void Dispose()
    {
        foreach (SKTypeface font in Fonts)
        {
            font.Dispose();
        }
    }
}

public record TextAndFont(string Text, SKTypeface Typeface);