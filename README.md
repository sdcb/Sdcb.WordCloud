# Sdcb.WordCloud [![NuGet](https://img.shields.io/nuget/v/Sdcb.WordCloud.svg)](https://nuget.org/packages/Sdcb.WordCloud)

**English** | **[简体中文](README_CN.md)**

**Sdcb.WordCloud** is a versatile, cross-platform library for generating word cloud images, SVGs, or JSON data based on word frequencies. It harnesses the power of SkiaSharp for graphical operations, ensuring high performance and quality without relying on System.Drawing. This makes it an excellent choice for applications running across various platforms, including server-side scenarios where GUI libraries might not be available.

## Key Features

- **Cross-platform compatibility**: Works across different operating systems without dependency on `System.Drawing`.
- **Multiple output formats**: Supports generating word clouds as images, SVGs, or JSON data.
- **Flexible configuration**: Customize your word clouds with various options, including text orientations, fonts, and masks.
- **Open-source**: Freely available under the MIT license, welcoming contributions and modifications.

## Installation

To start using `Sdcb.WordCloud` in your project, install it via NuGet:

```bash
dotnet add package Sdcb.WordCloud
```

## Usage Example

Below are some examples showcasing different capabilities of the Sdcb.WordCloud library. Note: The demonstrations use a shared word frequency list, as provided in the method `MakeDemoScore()`.

### Example 1: Different Text Orientations

This example demonstrates creating word clouds with 5 different text orientations.

```csharp
void Main()
{
    TextOrientations[] orientations = 
    [
        TextOrientations.PreferHorizontal, // default
        TextOrientations.PreferVertical, 
        TextOrientations.HorizontalOnly,
        TextOrientations.VerticalOnly, 
        TextOrientations.Random,
    ];
    foreach (var o in orientations)
    {
        WordCloud wc = WordCloud.Create(new WordCloudOptions(600, 600, MakeDemoScore())
        {
            TextOrientation = o,
        });
        byte[] pngBytes = wc.ToSKBitmap().Encode(SKEncodedImageFormat.Png, 100).AsSpan().ToArray();
        File.WriteAllBytes($"{o}.png", pngBytes);
    }
}
```

![image](https://github.com/sdcb/Sdcb.WordCloud/assets/1317141/ba754b19-82f4-4a26-9246-5a33007fbbcd)


### Example 2: Converting Word Cloud to JSON and Back

Generate a word cloud, convert it to JSON, and then reconstruct it to demonstrate the flexibility of manipulating word cloud data.

```csharp
void Main()
{
    WordCloud wc = WordCloud.Create(new WordCloudOptions(900, 900, MakeDemoScore())
    {
        FontManager = new FontManager([SKTypeface.FromFamilyName("Times New Roman")]),
        Mask = MaskOptions.CreateWithForegroundColor(SKBitmap.Decode(
            new HttpClient().GetByteArrayAsync("https://io.starworks.cc:88/cv-public/2024/alice_mask.png").GetAwaiter().GetResult()),
            SKColors.White)
    });
    string json = wc.ToJson();
    Console.WriteLine(json);

    // can convert back from generated json
    WordCloud wc2 = WordCloud.FromJson(json);
    File.WriteAllText($"json-convert-back.svg", wc2.ToSvg());
}
```

**Note**: This example illustrates how a word cloud's data can be stored in and reconstructed from JSON, enabling easy sharing and modification.

![image](https://github.com/sdcb/Sdcb.WordCloud/assets/1317141/8d7a7c5d-680c-49c7-ba4d-83c11436269e)

### Example 3: Applying a Mask

Creating a word cloud with a mask, allowing words to fill a specific shape.

```csharp
void Main()
{
    WordCloud wc = WordCloud.Create(new WordCloudOptions(900, 900, MakeDemoScore())
    {
        FontManager = new FontManager([SKTypeface.FromFamilyName("Times New Roman")]),
        Mask = MaskOptions.CreateWithForegroundColor(SKBitmap.Decode(
            new HttpClient().GetByteArrayAsync("https://io.starworks.cc:88/cv-public/2024/alice_mask.png").GetAwaiter().GetResult()),
            SKColors.White)
    });
    byte[] pngBytes = wc.ToSKBitmap().Encode(SKEncodedImageFormat.Png, 100).AsSpan().ToArray();
    File.WriteAllBytes($"mask.png", pngBytes);
}
```

![image](https://github.com/sdcb/Sdcb.WordCloud/assets/1317141/21de8c54-2088-4c08-a8ff-d4085880223f)


### Example 4: Using a Specific Font

Demonstrates how to utilize a specific font for the word cloud generation.

```csharp
void Main()
{
    WordCloud wc = WordCloud.Create(new WordCloudOptions(600, 600, MakeDemoScore())
    {
        FontManager = new FontManager([SKTypeface.FromFamilyName("Consolas")])
    });
    byte[] pngBytes = wc.ToSKBitmap().Encode(SKEncodedImageFormat.Png, 100).AsSpan().ToArray();
    File.WriteAllBytes($"specified-font.png", pngBytes);
}
```

![image](https://github.com/sdcb/Sdcb.WordCloud/assets/1317141/fa33e479-21f8-468e-b792-282990e51384)

### Example 5: Generating SVG Output

Shows how to generate an SVG from a word cloud, useful for web applications where scalability is crucial.

```csharp
void Main()
{
    WordCloud wc = WordCloud.Create(new WordCloudOptions(900, 900, MakeDemoScore())
    {
        FontManager = new FontManager([SKTypeface.FromFamilyName("Times New Roman")]),
        Mask = MaskOptions.CreateWithForegroundColor(SKBitmap.Decode(
            new HttpClient().GetByteArrayAsync("https://io.starworks.cc:88/cv-public/2024/alice_mask.png").GetAwaiter().GetResult()),
            SKColors.White)
    });
    File.WriteAllText($"json-convert-back.svg", wc.ToSvg());
    //new Svg(wc.ToSvg(), wc.Width, wc.Height).Dump();
}
```

![svg](./assets/demo.svg)

### Shared Word Frequency List

All examples use the following word frequency list:

```csharp
static IEnumerable<WordScore> MakeDemoScore()
{
    string text = """
        459    cloud
        112    Retrieved
        88    form
        78    species
        74    meteorolog
        66    type
        62    ed.
        54    edit
        53    cumulus
        52    World
        50    into
        50    level
        49    Organization
        49    air
        48    International
        48    atmosphere
        48    troposphere
        45    Atlas
        45    genus
        44    cirrus
        43    convection
        42    vertical
        40    altitude
        40    stratocumulus
        38    high
        38    weather
        37    climate
        37    layer
        36    cumulonimbus
        35    appear
        35    variety
        34    more
        34    water
        33    altocumulus
        33    feature
        33    low
        31    formation
        31    other
        28    precipitation
        28    produce
        27    .mw-parser-output
        27    name
        27    surface
        27    very
        26    base
        25    April
        25    ISBN
        25    also
        25    origin
        24    cirrocumulus
        24    color
        24    cool
        24    stratiformis
        24    stratus
        23    Earth
        23    structure
        22    altostratus
        22    cumuliform
        22    doi
        22    genera
        22    physics
        22    result
        22    see
        22    stratiform
        22    usual
        21    general
        21    p.
        21    seen
        21    temperature
        21    than
        20    polar
        20    space
        20    top
        19    Bibcode
        19    National
        19    cirrostratus
        19    fog
        19    lift
        19    mostly
        19    over
        18    Archived
        18    Latin
        18    Universe
        18    group
        18    sometimes
        18    supplementary
        18    tower
        18    warm
        17    January
        17    change
        17    light
        17    main
        17    nimbostratus
        17    occur
        17    only
        17    stratocumuliform
        17    unstable
        17    wind
        16    associated
        16    cause
        16    global
        16    instability
        16    mid-level
        16    nasa
        16    new
        16    noctilucent
        16    range
        16    science
        16    show
        16    tend
        15    accessory
        15    any
        15    cirriform
        15    droplets
        15    during
        15    extent
        15    multi-level
        15    near
        15    one
        15    rain
        15    reflect
        15    stratosphere
        15    used
        14    J.
        14    October
        14    PDF
        14    airmass
        14    become
        14    common
        14    converge
        14    crystal
        14    cyclone
        14    front
        14    high-level
        14    observes
        14    stable
        14    system
        14    white
        13    all
        13    along
        13    because
        13    classify
        13    each
        13    fibratus
        13    higher
        13    identify
        13    increase
        13    large
        13    mesosphere
        13    sun
        12    castellanus
        12    classification
        12    compose
        12    condenses
        12    congestus
        12    effect
        12    fractus
        12    heavy
        12    include
        12    low-level
        12    moderate
        12    process
        12    radiation
        12    resemble
        12    solar
        12    term
        12    vapor
        12    zone
        11    August
        11    Ci
        11    February
        11    July
        11    M.
        11    March
        11    November
        11    b.
        11    conditions
        11    different
        11    ground
        11    known
        11    lead
        11    most
        11    occasional
        11    often
        11    perlucidus
        11    pressure
        11    research
        11    satellite
        11    saturated
        11    severe
        11    storm
        11    thunderstorm
        11    tornado
        11    visible
        """;

    return text
        .Split("\n")
        .Select(x => x.Trim().Split("\t"))
        .Select(x => new WordScore(Score: int.Parse(x[0]), Word: x[1]));
}
```

## License

Sdcb.WordCloud is open-source software licensed under the MIT license.
