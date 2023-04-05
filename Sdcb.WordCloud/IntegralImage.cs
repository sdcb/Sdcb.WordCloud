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

        public unsafe void SaveDebugImage(string debugFile)
        {
            using Bitmap bitmap = ConvertToGrayscaleBitmap();
            bitmap.Save(debugFile);
        }

        public int[,] ConvertToIntegralImage()
        {
            int[,] nonIntegralImage = new int[Width, Height];

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    uint topLeft = (i > 0 && j > 0) ? Integral[j - 1, i - 1] : 0;
                    uint top = (i > 0) ? Integral[j, i - 1] : 0;
                    uint left = (j > 0) ? Integral[j - 1, i] : 0;
                    nonIntegralImage[j, i] = (int)(Integral[j, i] - left - top + topLeft);
                }
            }

            return nonIntegralImage;
        }

        public Bitmap ConvertToGrayscaleBitmap()
        {
            int[,] nonIntegralImage = ConvertToIntegralImage();
            Bitmap grayscaleBitmap = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);

            // 设置灰度调色板
            ColorPalette palette = grayscaleBitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            grayscaleBitmap.Palette = palette;

            BitmapData bitmapData = grayscaleBitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;
                int stride = bitmapData.Stride;

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int value = nonIntegralImage[x, y];
                        value = value switch
                        {
                            <= 0 => 0,
                            _ => 255
                        };
                        ptr[y * stride + x] = (byte)value;
                    }
                }
            }

            grayscaleBitmap.UnlockBits(bitmapData);

            return grayscaleBitmap;
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

                        uint pixel = masked ? 1u : 0;
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

        internal unsafe void Update(Bitmap image, Rectangle globalRect)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (image.PixelFormat != PixelFormat.Format32bppArgb) throw new ArgumentException($"Bitmap pixel format must be Format32bppArgb");

            BitmapData locked = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            try
            {
                ReadOnlySpan<byte> data = new(locked.Scan0.ToPointer(), locked.Stride * locked.Height);
                const int pixelFormatSize = 4;

                for (int yGlobal = globalRect.Y; yGlobal < Height; ++yGlobal)
                {
                    for (int xGlobal = globalRect.X; xGlobal < Width; ++xGlobal)
                    {
                        int yLocal = yGlobal - globalRect.Y;
                        int xLocal = xGlobal - globalRect.X;

                        if (xLocal < image.Width && yLocal < image.Height)
                        {
                            byte pixel = 0;
                            for (int p = 0; p < pixelFormatSize; ++p)
                            {
                                pixel |= data[yLocal * locked.Stride + xLocal * pixelFormatSize + p];
                            }
                            // Update the integral image only when the pixel is within the globalRect.
                            Integral[xGlobal, yGlobal] = pixel + Integral[xGlobal - 1, yGlobal] + Integral[xGlobal, yGlobal - 1] - Integral[xGlobal - 1, yGlobal - 1];
                        }
                        else
                        {
                            // When outside the globalRect, ensure the integral values remain continuous.
                            Integral[xGlobal, yGlobal] = Integral[xGlobal - 1, yGlobal] + Integral[xGlobal, yGlobal - 1] - Integral[xGlobal - 1, yGlobal - 1];
                        }
                    }
                }
            }
            finally
            {
                image.UnlockBits(locked);
            }
        }

        public ulong GetArea(int xPos, int yPos, int sizeX, int sizeY)
        {
            ulong area = Integral[xPos, yPos] + Integral[xPos + sizeX, yPos + sizeY];
            area -= Integral[xPos + sizeX, yPos] + Integral[xPos, yPos + sizeY];
            return area;
        }

        public bool IsMonotonicallyIncreasing()
        {
            for (int y = 0; y < Height - 1; y++)
            {
                for (int x = 0; x < Width - 1; x++)
                {
                    if (Integral[x, y] > Integral[x + 1, y] || Integral[x, y] > Integral[x, y + 1])
                    {
                        return false;
                    }
                }
            }

            // Check the last row
            for (int x = 0; x < Width - 1; x++)
            {
                if (Integral[x, Height - 1] > Integral[x + 1, Height - 1])
                {
                    return false;
                }
            }

            // Check the last column
            for (int y = 0; y < Height - 1; y++)
            {
                if (Integral[Width - 1, y] > Integral[Width - 1, y + 1])
                {
                    return false;
                }
            }

            return true;
        }

        public Point FindFirstNonIncreasingPoint()
        {
            for (int y = 0; y < Height - 1; y++)
            {
                for (int x = 0; x < Width - 1; x++)
                {
                    if (Integral[x, y] > Integral[x + 1, y])
                    {
                        return new(x + 1, y);
                    }
                    else if (Integral[x, y] > Integral[x, y + 1])
                    {
                        return new(x, y + 1);
                    }
                }
            }

            // Check the last row
            for (int x = 0; x < Width - 1; x++)
            {
                if (Integral[x, Height - 1] > Integral[x + 1, Height - 1])
                {
                    return new(x + 1, Height - 1);
                }
            }

            // Check the last column
            for (int y = 0; y < Height - 1; y++)
            {
                if (Integral[Width - 1, y] > Integral[Width - 1, y + 1])
                {
                    return new(Width - 1, y + 1);
                }
            }

            return new Point(-1, -1);
        }

        public int Width { get; set; }

        public int Height { get; set; }

        protected uint[,] Integral { get; set; }
    }
}
