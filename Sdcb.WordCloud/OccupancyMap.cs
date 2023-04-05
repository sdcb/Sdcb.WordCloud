using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sdcb.WordClouds
{
    internal class OccupancyMap : IntegralImage
    {
        private readonly Random _rand;

        public OccupancyMap(int width, int height, Random random) : base(width, height)
        {
            _rand = random;
        }

        public bool TryFindUnoccupiedPosition(int sizeX, int sizeY, out int oPosX, out int oPosY)
        {
            oPosX = -1;
            oPosY = -1;

            int startPosX = _rand.Next(1, Width);
            int startPosY = _rand.Next(1, Height);

            int x, y, dx, dy;
            x = y = dx = 0;
            dy = -1;
            int width = Width - sizeX;
            int height = Height - sizeY;

            int maxI = (int)Math.Pow(Math.Max(width, height), 2);

            for (int i = 0; i < maxI; i++)
            {
                if ((-width / 2 <= x) && (x <= width / 2) && (-height / 2 <= y) && (y <= height / 2))
                {
                    int posX = x + startPosX;
                    int posY = y + startPosY;
                    if (posY > 0 && posY < Height - sizeY && posX > 0 && posX < Width - sizeX)
                    {
                        if (GetArea(posX, posY, sizeX, sizeY) == 0)
                        {
                            oPosX = posX;
                            oPosY = posY;
                            return true;
                        }
                    }
                }
                if ((x == y) || ((x < 0) && (x == -y)) || ((x > 0) && (x == 1 - y)))
                {
                    int t = dx;
                    dx = -dy;
                    dy = t;
                }
                x += dx;
                y += dy;
            }

            return false;
        }
    }
}
