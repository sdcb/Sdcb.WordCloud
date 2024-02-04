using Sdcb.WordClouds;
using SkiaSharp;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Sdcb.WordClouds.Tests;

public class ToBitmapTest
{
    private static readonly string OutputDirectory = "WordCloudOutputs";
    private readonly ITestOutputHelper _console;

    public ToBitmapTest(ITestOutputHelper console)
    {
        _console = console;
        if (!Directory.Exists(OutputDirectory))
        {
            Directory.CreateDirectory(OutputDirectory);
        }
    }

    //[Fact]
    //public void TestWordCloudCreation_SimpleCase()
    //{
    //    var wordCloud = new WordCloud(200, 100, new FontManager(["Arial"]),
    //    [
    //        new TextLine("Hello", 24, SKColors.Black, new SKPoint(100, 30), 0)
    //    ]);

    //    TestWordCloudBitmap(wordCloud, "TestWordCloudCreation_SimpleCase.png");
    //}

    //[Fact]
    //public void TestWordCloudCreation_MultipleWords()
    //{
    //    var wordCloud = new WordCloud(400, 200, new FontManager(["Times New Roman"]),
    //    [
    //        new TextLine("Multiple", 30, SKColors.Blue, new SKPoint(200, 35), 0),
    //        new TextLine("Words", 30, SKColors.Red, new SKPoint(200, 95), 0)
    //    ]);

    //    TestWordCloudBitmap(wordCloud, "TestWordCloudCreation_MultipleWords.png");
    //}

    //[Theory]
    //[InlineData(0)]
    //[InlineData(10)]
    //[InlineData(45)]
    //[InlineData(90)]
    //public void TestWordCloudCreation_RotatedText(float rotate)
    //{
    //    var wordCloud = new WordCloud(300, 300, new FontManager(["Courier New"]),
    //    [
    //        new TextLine("这是参考文字", 24, SKColors.Green, new SKPoint(150, 45), 0),
    //        new TextLine("Rotated", 24, SKColors.Red, new SKPoint(165, 120), rotate)
    //    ]);

    //    TestWordCloudBitmap(wordCloud, $"TestWordCloudCreation_RotatedText_{rotate}.png");
    //}

    //[Fact]
    //public void TestWordCloudCreation_PerfTest()
    //{
    //    WordCloud wordCloud = new (800, 600, new FontManager(["Courier New"]),
    //    [
    //        new TextLine(".NET", 50, SKColors.Blue, new SKPoint(400, 60), 90),
    //        new TextLine("C#", 46, SKColors.Purple, new SKPoint(200, 100), 0),
    //        new TextLine("LINQ", 40, SKColors.Green, new SKPoint(600, 120), 90),
    //        new TextLine("ASP.NET", 38, SKColors.Teal, new SKPoint(150, 150), 0),
    //        new TextLine("Entity Framework", 34, SKColors.Orange, new SKPoint(650, 170), 0),
    //        new TextLine("Visual Studio", 28, SKColors.Red, new SKPoint(420, 220), 90),
    //        new TextLine("MVC", 26, SKColors.DarkBlue, new SKPoint(300, 260), 0),
    //        new TextLine("NuGet", 24, SKColors.Chocolate, new SKPoint(500, 280), 90),
    //        new TextLine("Xamarin", 22, SKColors.Crimson, new SKPoint(240, 330), 0),
    //        new TextLine("WPF", 22, SKColors.DarkCyan, new SKPoint(540, 330), 90),
    //        new TextLine("Blazor", 20, SKColors.DarkGreen, new SKPoint(100, 440), 0),
    //        new TextLine("F#", 20, SKColors.MediumVioletRed, new SKPoint(280, 440), 90),
    //        new TextLine("CLR", 20, SKColors.DarkGoldenrod, new SKPoint(460, 440), 0),
    //        new TextLine("Delegates", 20, SKColors.MidnightBlue, new SKPoint(640, 440), 0),
    //        new TextLine("Razor", 18, SKColors.OliveDrab, new SKPoint(150, 480), 90),
    //        new TextLine("Attributes", 18, SKColors.DarkMagenta, new SKPoint(400, 480), 0),
    //        new TextLine("Threads", 18, SKColors.SteelBlue, new SKPoint(650, 480), 0),
    //        new TextLine("IQueryable", 16, SKColors.SlateGray, new SKPoint(200, 530), 90),
    //        new TextLine("Garbage Collection", 16, SKColors.Sienna, new SKPoint(500, 530), 0),
    //        new TextLine("Reflection", 16, SKColors.DarkOliveGreen, new SKPoint(350, 570), 0),
    //        new TextLine("Lambdas", 16, SKColors.DarkSlateBlue, new SKPoint(600, 570), 90)
    //    ]);
    //    int acc = 0;
    //    int times = 5;
    //    for (int i = 0; i < times; ++i)
    //    {
    //        Stopwatch sw = Stopwatch.StartNew();
    //        using SKBitmap bmp = wordCloud.ToSKBitmap();
    //        _console.WriteLine($"ToBitmap elapsed={sw.ElapsedMilliseconds}ms");
    //        acc += (int)sw.ElapsedMilliseconds;
    //    }
    //    _console.WriteLine($"Average elapsed={acc / times}ms");

    //    {
    //        using SKBitmap bmp = wordCloud.ToSKBitmap(addBox: true);
    //        using SKImage image = SKImage.FromBitmap(bmp);
    //        string dest = Path.Combine(OutputDirectory, "TestWordCloudCreation_PerfTest.png");
    //        _console.WriteLine($"dest: {dest}");
    //        image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(File.OpenWrite(dest));
    //    }
    //}

    private static void TestWordCloudBitmap(WordCloud wordCloud, string fileName)
    {
        using SKBitmap bitmap = wordCloud.ToSKBitmap(addBox: true);

        string filePath = Path.Combine(OutputDirectory, fileName);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(filePath);
        data.SaveTo(stream);
    }
}
