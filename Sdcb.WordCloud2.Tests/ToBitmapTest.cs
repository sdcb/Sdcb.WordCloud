using Sdcb.WordClouds;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdcb.WordCloud2.Tests;

public class ToBitmapTest
{
    private static readonly string OutputDirectory = "WordCloudOutputs";

    public ToBitmapTest()
    {
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
            new TextItem("Hello", 24, SKColors.Black, new SKRect(10, 10, 190, 50), 0)
        ]);

        TestWordCloudBitmap(wordCloud, "TestWordCloudCreation_SimpleCase.png");
    }

    [Fact]
    public void TestWordCloudCreation_MultipleWords()
    {
        var wordCloud = new WordCloud(400, 200, ["Times New Roman"],
        [
            new TextItem("Multiple", 30, SKColors.Blue, new SKRect(10, 10, 390, 60), 0),
            new TextItem("Words", 30, SKColors.Red, new SKRect(10, 70, 390, 120), 0)
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
            new TextItem("Ref", 24, SKColors.Green, new SKRect(0, 0, 270, 90), 0),
            new TextItem("Rotated", 24, SKColors.Red, new SKRect(60, 0, 270, 90), rotate)
        ]);

        TestWordCloudBitmap(wordCloud, $"TestWordCloudCreation_RotatedText_{rotate}.png");
    }

    private void TestWordCloudBitmap(WordCloud wordCloud, string fileName)
    {
        using SKBitmap bitmap = wordCloud.ToBitmap(addBox: true);

        string filePath = Path.Combine(OutputDirectory, fileName);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(filePath);
        data.SaveTo(stream);
    }
}
