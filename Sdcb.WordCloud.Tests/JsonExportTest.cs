using SkiaSharp;
using Xunit.Abstractions;

namespace Sdcb.WordClouds.Tests;

public class JsonExportTest
{
    private static readonly string OutputDirectory = "WordCloudOutputs";
    private readonly ITestOutputHelper _console;

    public JsonExportTest(ITestOutputHelper console)
    {
        _console = console;
        if (!Directory.Exists(OutputDirectory))
        {
            Directory.CreateDirectory(OutputDirectory);
        }
    }

    [Fact]
    public void HorizontalTest()
    {
        WordCloud cloud = WordCloud.FromJson(GetHorizontalSvgJson());
        string svg = cloud.ToSvg();
        File.WriteAllText(Path.Combine(OutputDirectory, "hori.svg"), svg);
        using SKBitmap bmp = cloud.ToSKBitmap();
        File.WriteAllBytes(Path.Combine(OutputDirectory, "hori.png"), bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray());
    }

    [Fact]
    public void VerticalTest()
    {
        WordCloud cloud = WordCloud.FromJson(GetVerticalSvgJson());
        string svg = cloud.ToSvg();
        File.WriteAllText(Path.Combine(OutputDirectory, "vert.svg"), svg);
        using SKBitmap bmp = cloud.ToSKBitmap();
        File.WriteAllBytes(Path.Combine(OutputDirectory, "vert.png"), bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray());
    }

    static string GetHorizontalSvgJson() => """
        {
          "width": 500,
          "height": 500,
          "textLines": [
            {
              "textGroup": {
                "texts": [
                  {
                    "text": "cloud",
                    "typeface": "Segoe UI",
                    "width": 498.1114,
                    "left": 0,
                    "height": 270.98264
                  }
                ]
              },
              "fontSize": 203.74118,
              "color": "#15AEA2",
              "center": [
                249,
                135
              ],
              "rotate": 0
            },
            {
              "textGroup": {
                "texts": [
                  {
                    "text": "Retrieved",
                    "typeface": "Segoe UI",
                    "width": 498.5513,
                    "left": 0,
                    "height": 159.25607
                  }
                ]
              },
              "fontSize": 119.74118,
              "color": "#947BCB",
              "center": [
                249,
                302
              ],
              "rotate": 0
            }
          ],
          "background": null
        }
        """;

    static string GetVerticalSvgJson() => """
        {
          "width": 500,
          "height": 500,
          "textLines": [
            {
              "textGroup": {
                "texts": [
                  {
                    "text": "cloud",
                    "typeface": "Segoe UI",
                    "width": 498.1114,
                    "left": 0,
                    "height": 270.98264
                  }
                ]
              },
              "fontSize": 203.74118,
              "color": "#15AEA2",
              "center": [
                135,
                249
              ],
              "rotate": 90
            },
            {
              "textGroup": {
                "texts": [
                  {
                    "text": "Retrieved",
                    "typeface": "Segoe UI",
                    "width": 498.5513,
                    "left": 0,
                    "height": 159.25607
                  }
                ]
              },
              "fontSize": 119.74118,
              "color": "#947BCB",
              "center": [
                282,
                249
              ],
              "rotate": 90
            }
          ],
          "background": null
        }
        """;
}
