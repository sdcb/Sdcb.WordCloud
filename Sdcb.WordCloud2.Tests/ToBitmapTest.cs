using Sdcb.WordClouds;
using SkiaSharp;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Sdcb.WordCloud2.Tests;

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

    [Fact]
    public void TestWordCloudCreation_SimpleCase()
    {
        var wordCloud = new WordCloud(200, 100, ["Arial"],
        [
            new TextItem("Hello", 24, SKColors.Black, new SKPoint(100, 30), 0)
        ]);

        TestWordCloudBitmap(wordCloud, "TestWordCloudCreation_SimpleCase.png");
    }

    [Fact]
    public void TestWordCloudCreation_MultipleWords()
    {
        var wordCloud = new WordCloud(400, 200, ["Times New Roman"],
        [
            new TextItem("Multiple", 30, SKColors.Blue, new SKPoint(200, 35), 0),
            new TextItem("Words", 30, SKColors.Red, new SKPoint(200, 95), 0)
        ]);

        TestWordCloudBitmap(wordCloud, "TestWordCloudCreation_MultipleWords.png");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(45)]
    [InlineData(90)]
    public void TestWordCloudCreation_RotatedText(float rotate)
    {
        var wordCloud = new WordCloud(300, 300, ["Courier New"],
        [
            new TextItem("这是参考文字", 24, SKColors.Green, new SKPoint(150, 45), 0),
            new TextItem("Rotated", 24, SKColors.Red, new SKPoint(165, 120), rotate)
        ]);

        TestWordCloudBitmap(wordCloud, $"TestWordCloudCreation_RotatedText_{rotate}.png");
    }

    [Fact]
    public void TestWordCloudCreation_PerfTest()
    {
        WordCloud wordCloud = new (800, 600, ["Courier New"],
        [
            new TextItem(".NET", 50, SKColors.Blue, new SKPoint(400, 60), 90),
            new TextItem("C#", 46, SKColors.Purple, new SKPoint(200, 100), 0),
            new TextItem("LINQ", 40, SKColors.Green, new SKPoint(600, 120), 90),
            new TextItem("ASP.NET", 38, SKColors.Teal, new SKPoint(150, 150), 0),
            new TextItem("Entity Framework", 34, SKColors.Orange, new SKPoint(650, 170), 0),
            new TextItem("Visual Studio", 28, SKColors.Red, new SKPoint(420, 220), 90),
            new TextItem("MVC", 26, SKColors.DarkBlue, new SKPoint(300, 260), 0),
            new TextItem("NuGet", 24, SKColors.Chocolate, new SKPoint(500, 280), 90),
            new TextItem("Xamarin", 22, SKColors.Crimson, new SKPoint(240, 330), 0),
            new TextItem("WPF", 22, SKColors.DarkCyan, new SKPoint(540, 330), 90),
            new TextItem("Blazor", 20, SKColors.DarkGreen, new SKPoint(100, 440), 0),
            new TextItem("F#", 20, SKColors.MediumVioletRed, new SKPoint(280, 440), 90),
            new TextItem("CLR", 20, SKColors.DarkGoldenrod, new SKPoint(460, 440), 0),
            new TextItem("Delegates", 20, SKColors.MidnightBlue, new SKPoint(640, 440), 0),
            new TextItem("Razor", 18, SKColors.OliveDrab, new SKPoint(150, 480), 90),
            new TextItem("Attributes", 18, SKColors.DarkMagenta, new SKPoint(400, 480), 0),
            new TextItem("Threads", 18, SKColors.SteelBlue, new SKPoint(650, 480), 0),
            new TextItem("IQueryable", 16, SKColors.SlateGray, new SKPoint(200, 530), 90),
            new TextItem("Garbage Collection", 16, SKColors.Sienna, new SKPoint(500, 530), 0),
            new TextItem("Reflection", 16, SKColors.DarkOliveGreen, new SKPoint(350, 570), 0),
            new TextItem("Lambdas", 16, SKColors.DarkSlateBlue, new SKPoint(600, 570), 90)
        ]);
        int acc = 0;
        int times = 5;
        for (int i = 0; i < times; ++i)
        {
            Stopwatch sw = Stopwatch.StartNew();
            using SKBitmap bmp = wordCloud.ToBitmap();
            _console.WriteLine($"ToBitmap elapsed={sw.ElapsedMilliseconds}ms");
            acc += (int)sw.ElapsedMilliseconds;
        }
        _console.WriteLine($"Average elapsed={acc / times}ms");

        {
            using SKBitmap bmp = wordCloud.ToBitmap(addBox: true);
            using SKImage image = SKImage.FromBitmap(bmp);
            string dest = Path.Combine(OutputDirectory, "TestWordCloudCreation_PerfTest.png");
            _console.WriteLine($"dest: {dest}");
            image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(File.OpenWrite(dest));
        }
    }

    private static void TestWordCloudBitmap(WordCloud wordCloud, string fileName)
    {
        using SKBitmap bitmap = wordCloud.ToBitmap(addBox: true);

        string filePath = Path.Combine(OutputDirectory, fileName);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(filePath);
        data.SaveTo(stream);
    }
}
