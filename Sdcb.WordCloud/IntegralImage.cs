using System;
using System.Drawing;
using System.Drawing.Imaging;

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

        public void Reset()
        {
            for (int i = 0; i < Width; ++i)
            {
                for (int j = 0; j < Height; ++j)
                {
                    Integral[i, j] = 0;
                }
            }
        }

        public unsafe void UpdateBitmapMask(Bitmap image, byte maskThreshold)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (image.Width != Width || image.Height != Height)
                throw new ArgumentException("Image size does not match integral image size.", nameof(image));

            BitmapData dataRect = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            try
            {
                int pixelFormatSize = dataRect.Stride / dataRect.Width;
                ReadOnlySpan<byte> data = new(dataRect.Scan0.ToPointer(), dataRect.Stride * dataRect.Height);
                for (int i = 1; i < dataRect.Height; ++i)
                {
                    for (int j = 1; j < dataRect.Width; ++j)
                    {
                        bool containsWordCloud = true;
                        for (int p = 0; p < pixelFormatSize; ++p)
                        {
                            if (data[i * dataRect.Stride + j * pixelFormatSize + p] < maskThreshold)
                            {
                                containsWordCloud = false;
                                continue;
                            }
                        }
                        byte pixel = containsWordCloud ? (byte)255 : (byte)0;
                        Integral[j, i] = pixel + Integral[j - 1, i] + Integral[j, i - 1] - Integral[j - 1, i - 1];
                    }
                }
            }
            finally
            {
                image.UnlockBits(dataRect);
            }
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
