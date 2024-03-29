﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdcb.WordClouds;

internal static class Utils
{
    public static T[] Convert2DTo1D<T>(T[,] data)
    {
        int rows = data.GetLength(0);
        int columns = data.GetLength(1);
        T[] flatArray = new T[rows * columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                flatArray[i * columns + j] = data[i, j];
            }
        }

        return flatArray;
    }
}
