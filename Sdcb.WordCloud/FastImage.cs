using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
[assembly: InternalsVisibleTo("Sdcb.WordClouds.Tests")]

namespace Sdcb.WordClouds
{
    /// <summary>
    /// Image class for fast manipulation of bitmap data.
    /// </summary>
    internal class FastImage : IDisposable
    {
        public FastImage(int width, int height, PixelFormat format)
        {
            Width = width;
            Height = height;
            Format = format;
            PixelFormatSize = Image.GetPixelFormatSize(Format) / 8;
            Stride = Width * PixelFormatSize;
            AllocatedSize = Stride * Height;

            Pointer = Marshal.AllocHGlobal(AllocatedSize);
            GC.AddMemoryPressure(AllocatedSize);
        }

        public int Width { get; }

        public int Height { get; }

        public PixelFormat Format { get; }

        public int PixelFormatSize { get; }

        public int Stride { get; }

        public int AllocatedSize { get; }

        public IntPtr Pointer { get; private set; }

        public unsafe Span<byte> CreateDataAccessor()
        {
            return new Span<byte>((void*)Pointer, AllocatedSize);
        }

        public unsafe void Reset()
        {
            Span<byte> data = CreateDataAccessor();
            data.Fill(0);
        }

        public Bitmap CreateBitmap()
        {
            return new Bitmap(Width, Height, Stride, Format, Pointer);
        }

        ~FastImage()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Pointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Pointer);
                GC.RemoveMemoryPressure(AllocatedSize);
                Pointer = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
