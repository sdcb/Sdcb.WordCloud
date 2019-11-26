using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Sdcb.WordClouds
{
	/// <summary>
	/// Image class for fast manipulation of bitmap data.
	/// </summary>
	internal class FastImage : IDisposable
	{
		public FastImage(int width, int height, PixelFormat format)
		{
			PixelFormatSize = Image.GetPixelFormatSize(format) / 8;
			Stride = width*PixelFormatSize;

			Data = new byte[Stride * height];
			Handle = GCHandle.Alloc(Data, GCHandleType.Pinned);
			IntPtr pData = Marshal.UnsafeAddrOfPinnedArrayElement(Data, 0);
			Bitmap = new Bitmap(width, height, Stride, format, pData);
		}

		public int Width { get { return Bitmap.Width; } }

		public int Height { get { return Bitmap.Height; } }

		public int PixelFormatSize { get; set; }

		public GCHandle Handle { get; set; }

		public int Stride { get; set; }

		public byte[] Data { get; set; }

		public Bitmap Bitmap { get; set; }
		public void Dispose()
		{
			Handle.Free();
			Bitmap.Dispose();
		}
	}
}
