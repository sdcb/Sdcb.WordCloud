using Sdcb.WordClouds;

namespace Sdcb.WordCloud2.Tests;

public class UpdateIntegralTest
{
    [Fact]
    public void UpdateIntegral_Should_Throw_If_Sizes_Differ()
    {
        bool[,] cache = new bool[2, 3];
        int[,] integral = new int[2, 2]; // Intentionally wrong size

        var exception = Record.Exception(() => WordCloudFactory.UpdateIntegral(cache, integral));

        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
        Assert.Equal("Cache size does not match the integral size.", exception.Message);
    }

    [Fact]
    public void UpdateIntegral_With_All_False_Should_Produce_All_Zeroes_Integral()
    {
        bool[,] cache = new bool[2, 2] 
        { 
            { false, false }, 
            { false, false } 
        };
        int[,] integral = new int[2, 2];

        WordCloudFactory.UpdateIntegral(cache, integral);

        Assert.Equal(0, integral[0, 0]);
        Assert.Equal(0, integral[0, 1]);
        Assert.Equal(0, integral[1, 0]);
        Assert.Equal(0, integral[1, 1]);
    }

    [Fact]
    public void UpdateIntegral_With_All_True_Should_Produce_Correct_Integral()
    {
        bool[,] cache = new bool[2, 2] { { true, true }, { true, true } };
        int[,] integral = new int[2, 2];

        WordCloudFactory.UpdateIntegral(cache, integral);

        Assert.Equal(1, integral[0, 0]);
        Assert.Equal(2, integral[0, 1]);
        Assert.Equal(2, integral[1, 0]);
        Assert.Equal(4, integral[1, 1]);
    }

    [Fact]
    public void UpdateIntegral_With_Mixed_Values_Should_Produce_Correct_Integral()
    {
        bool[,] cache = new bool[3, 3] {
        { true, false, true },
        { false, true, false },
        { true, false, true }
    };
        int[,] integral = new int[3, 3];
        int[] expectedIntegralFlat =
        [
            1, 1, 2,
            1, 2, 3,
            2, 3, 5
        ];

        WordCloudFactory.UpdateIntegral(cache, integral);

        Assert.Equal(expectedIntegralFlat, Utils.Convert2DTo1D(integral));
    }

    [Fact]
    public void UpdateIntegral_With_NonSquareGridAndMixedValues_Should_Produce_Correct_Integral()
    {
        bool[,] cache = new bool[3, 4]
        {
            { true, false, true, false },
            { false, true, true, false },
            { true, false, false, true }
        };
        int[,] integral = new int[3, 4];
        int[] expectedIntegralFlat = 
        [
            1, 1, 2, 2,
            1, 2, 4, 4,
            2, 3, 5, 6
        ];

        WordCloudFactory.UpdateIntegral(cache, integral);

        Assert.Equal(expectedIntegralFlat, Utils.Convert2DTo1D(integral));
    }

    [Fact]
    public void UpdateIntegral_With_AllTrueValues_Should_Produce_Correct_Integral()
    {
        bool[,] cache = new bool[3, 3]
        {
            { true, true, true },
            { true, true, true },
            { true, true, true }
        };
        int[,] integral = new int[3, 3];
        int[] expectedIntegralFlat =
        [
            1, 2, 3,
            2, 4, 6,
            3, 6, 9
        ];

        WordCloudFactory.UpdateIntegral(cache, integral);

        Assert.Equal(expectedIntegralFlat, Utils.Convert2DTo1D(integral));
    }
}
