namespace Sdcb.WordClouds.Tests;

public class UpdateIntegralTest
{
    [Fact]
    public void UpdateIntegral_Should_Throw_If_Sizes_Differ()
    {
        bool[,] cache = new bool[2, 3];
        IntegralMap integralMap = new(2, 2);

        var exception = Record.Exception(() => integralMap.Update(cache));

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
        IntegralMap integral = new(2, 2);

        integral.Update(cache);

        Assert.Equal(0, integral[0, 0]);
        Assert.Equal(0, integral[0, 1]);
        Assert.Equal(0, integral[1, 0]);
        Assert.Equal(0, integral[1, 1]);
    }

    [Fact]
    public void UpdateIntegral_With_All_True_Should_Produce_Correct_Integral()
    {
        bool[,] cache = new bool[2, 2] { { true, true }, { true, true } };
        IntegralMap integral = new(2, 2);

        integral.Update(cache);

        Assert.Equal(1, integral[0, 0]);
        Assert.Equal(2, integral[0, 1]);
        Assert.Equal(2, integral[1, 0]);
        Assert.Equal(4, integral[1, 1]);
    }

    [Fact]
    public void UpdateIntegral_With_Mixed_Values_Should_Produce_Correct_Integral()
    {
        bool[,] cache = new bool[3, 3] 
        {
            { true, false, true },
            { false, true, false },
            { true, false, true }
        };
        IntegralMap integral = new(3, 3);
        int[] expectedIntegralFlat =
        [
            1, 1, 2,
            1, 2, 3,
            2, 3, 5
        ];

        integral.Update(cache);

        Assert.Equal(expectedIntegralFlat, integral.ToArray());
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
        IntegralMap integral = new (4, 3);
        int[] expectedIntegralFlat = 
        [
            1, 1, 2, 2,
            1, 2, 4, 4,
            2, 3, 5, 6
        ];

        integral.Update(cache);

        Assert.Equal(expectedIntegralFlat, integral.ToArray());
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
        IntegralMap integral = new(3, 3);
        int[] expectedIntegralFlat =
        [
            1, 2, 3,
            2, 4, 6,
            3, 6, 9
        ];

        integral.Update(cache);

        Assert.Equal(expectedIntegralFlat, integral.ToArray());
    }
}
