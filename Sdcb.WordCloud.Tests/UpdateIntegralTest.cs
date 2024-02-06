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

    [Fact]
    public void UpdatePartialIntegral_Should_Throw_If_StartCoordinates_Are_OutOfRange()
    {
        bool[,] cache = new bool[2, 2];
        IntegralMap integralMap = new(2, 2);

        var exception = Record.Exception(() => integralMap.Update(cache, 3, 3)); // 非法的起始坐标

        Assert.NotNull(exception);
        Assert.IsType<ArgumentOutOfRangeException>(exception);
    }

    [Fact]
    public void UpdatePartialIntegral_Should_Update_From_StartCoordinates_Onward()
    {
        bool[,] cache = new bool[3, 3] {
            { true, false, true },
            { false, false, true },
            { true, true, false }
        };
        IntegralMap integralMap = new(3, 3);
        integralMap.Update(cache); // 先生成完整的积分图
        Assert.Equal(
        [
            1,1,2,
            1,1,3,
            2,3,5
        ], integralMap.ToArray());

        // 更新局部区域[1,1]，新的值为false -> true
        cache[1, 1] = true;
        integralMap.Update(cache, 1, 1);

        int[] expectedIntegralFlat =
        [
            1,1,2,
            1,2,4,
            2,4,6
        ];
        int[] actual = integralMap.ToArray();
        Assert.Equal(expectedIntegralFlat, actual);
    }

    [Fact]
    public void UpdatePartialIntegral_Should_Not_Update_LeftAndTop_From_StartCoordinates_Onward()
    {
        bool[,] cache = new bool[3, 3] {
            { true, true, true },
            { true, false, true },
            { true, true, true }
        };
        IntegralMap integralMap = new(3, 3);
        integralMap.Update(cache); // 先生成完整的积分图

        // 更新局部区域[1,1]，之前是false的位置变成true
        cache[1, 1] = true;
        integralMap.Update(cache, 1, 1);

        int[] expectedIntegralFlat =
        {
            1, 2, 3,
            2, 4, 6, // 只有[1,1]点的值从原来的2更新为4
            3, 6, 9  // 这一行的积分值不发生改变
        };

        Assert.Equal(expectedIntegralFlat, integralMap.ToArray());
    }

    [Fact]
    public void UpdatePartialIntegral_With_Update_At_Edge_Should_Update_Correctly()
    {
        bool[,] cache = new bool[3, 3] {
            { true, false, false },
            { false, true, false },
            { false, false, true }
        };
        IntegralMap integralMap = new(3, 3);
        integralMap.Update(cache); // 先生成完整的积分图

        // 更新最右下角的局部区域[2,2]
        cache[2, 2] = false;
        integralMap.Update(cache, 2, 2);

        int[] expectedIntegralFlat =
        {
            1, 1, 1,
            1, 2, 2,
            1, 2, 2  // 注意最右下角的值从原来的3变成了2
        };

        Assert.Equal(expectedIntegralFlat, integralMap.ToArray());
    }
}
