using System;

namespace Sdcb.WordClouds
{
    internal class IntegralImage
    {
        public IntegralImage(int width, int height)
        {
            Integral = new uint[width, height];
            Width = width;
            Height = height;
        }

        public void Update(FastImage image)
        {
            Update(image, 1, 1);
        }

        public void Update(FastImage image, int posX, int posY)
        {
            if (posX < 1) posX = 1;
            if (posY < 1) posY = 1;

            Span<byte> data = image.CreateDataAccessor();
            for (int i = posY; i < image.Height; ++i)
            {
                for (int j = posX; j < image.Width; ++j)
                {
                    byte pixel = 0;
                    for (int p = 0; p < image.PixelFormatSize; ++p)
                    {
                        pixel |= data[i * image.Stride + j * image.PixelFormatSize + p];
                    }
                    Integral[j, i] = pixel + Integral[j - 1, i] + Integral[j, i - 1] - Integral[j - 1, i - 1];
                }
            }
        }

        public ulong GetArea(int xPos, int yPos, int sizeX, int sizeY)
        {
            ulong area = Integral[xPos, yPos] + Integral[xPos + sizeX, yPos + sizeY];
            area -= Integral[xPos + sizeX, yPos] + Integral[xPos, yPos + sizeY];
            return area;
        }

        public int Width { get; set; }

        public int Height { get; set; }

        protected uint[,] Integral { get; set; }
    }
}
