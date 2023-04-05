using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

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

        public void Reset(uint n = 0)
        {
            for (int i = 0; i < Width; ++i)
            {
                for (int j = 0; j < Height; ++j)
                {
                    Integral[i, j] = n;
                }
            }
        }

        public unsafe void SaveDebug(string debugFile)
        {
            StringBuilder sb = new StringBuilder();

            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    sb.Append(Integral[x, y]);
                    sb.Append(' ');
                }
                sb.AppendLine();
            }
            File.WriteAllText(debugFile, sb.ToString());
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
                for (int y = 0; y < dataRect.Height; ++y)
                {
                    for (int x = 0; x < dataRect.Width; ++x)
                    {
                        bool masked = false;
                        for (int p = 0; p < pixelFormatSize; ++p)
                        {
                            if (data[y * dataRect.Stride + x * pixelFormatSize + p] < maskThreshold)
                            {
                                masked = true;
                                continue;
                            }
                        }

                        uint pixel = masked ? 1u : 0u;
                        if (x == 0 && y == 0)
                        {
                            Integral[x, y] = pixel;
                        }
                        else if (x == 0)
                        {
                            Integral[x, y] = pixel + Integral[x, y - 1];
                        }
                        else if (y == 0)
                        {
                            Integral[x, y] = pixel + Integral[x - 1, y];
                        }
                        else
                        {
                            Integral[x, y] = pixel + Integral[x - 1, y] + Integral[x, y - 1] - Integral[x - 1, y - 1];
                        }
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
            for (int y = posY; y < image.Height; ++y)
            {
                for (int x = posX; x < image.Width; ++x)
                {
                    byte pixel = 0;
                    for (int p = 0; p < image.PixelFormatSize; ++p)
                    {
                        pixel |= data[y * image.Stride + x * image.PixelFormatSize + p];
                    }
                    Integral[x, y] = pixel + Integral[x - 1, y] + Integral[x, y - 1] - Integral[x - 1, y - 1];
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
