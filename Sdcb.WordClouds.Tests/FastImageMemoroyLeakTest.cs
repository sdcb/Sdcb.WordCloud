using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Sdcb.WordClouds;
using System.Drawing;
using System.Drawing.Imaging;
using Xunit.Abstractions;

namespace Sdcb.WordClouds.Tests
{
    public class FastImageMemoroyLeakTest
    {
        private readonly ITestOutputHelper _console;

        public FastImageMemoroyLeakTest(ITestOutputHelper console)
        {
            _console = console;
        }

        [Fact]
        public void MemoryLeakForFastImage()
        {
            int width = 100;
            int height = 100;
            PixelFormat pixelFormat = PixelFormat.Format32bppArgb;
            int pixelFormatSize = Image.GetPixelFormatSize(pixelFormat) / 8;
            int totalImageSize = width * height * pixelFormatSize;

            long m = MeasureMemoryAllocated(() =>
            {
                for (int i = 0; i < 800; ++i)
                {
                    FastImage fastImage = new(100, 100, PixelFormat.Format32bppArgb);
                }
            });
            _console.WriteLine(m.ToString("N0"));
            Assert.InRange(m, 0, totalImageSize * 2);
        }

        public long MeasureMemoryAllocated(Action action)
        {
            long m1 = GC.GetTotalMemory(true);
            try
            {
                action();
            }
            catch(Exception ) { }
            long m2 = GC.GetTotalMemory(false);
            return m2 - m1;
        }
    }
}