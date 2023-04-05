using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace Sdcb.WordClouds
{
    /// <summary>
    /// Class to draw word clouds.
    /// </summary>
    public class WordCloud
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WordCloud"/> class.
        /// </summary>
        /// <param name="width">The width of word cloud.</param>
        /// <param name="height">The height of word cloud.</param>
        /// <param name="useRank">if set to <c>true</c> will ignore frequencies for best fit.</param>
        /// <param name="fontColor">Color of the font.</param>
        /// <param name="maxFontSize">Maximum size of the font.</param>
        /// <param name="fontStep">The font step to use.</param>
        /// <param name="randomSeed">
        /// <para>The random seed to generate, random if provided null</para>
        /// <para>Note: same random seed will generate same WordCloud</para>
        /// </param>
        public WordCloud(int width, int height,
            bool useRank = false,
            Color? fontColor = null,
            int maxFontSize = -1,
            int fontStep = 1,
            int? randomSeed = null)
        {
            Width = width;
            Height = height;
            _random = randomSeed == null ? new Random() : new Random(randomSeed.Value);

            Map = new OccupancyMap(width, height, _random);

            MaxFontSize = maxFontSize < 0 ? height : maxFontSize;
            FontStep = fontStep;
            m_fontColor = fontColor;
            UseRank = useRank;
        }


        /// <summary>
        /// Draws the specified word cloud given list of words and frequecies
        /// </summary>
        /// <returns>Image of word cloud.</returns>
        /// <exception cref="System.ArgumentException">
        /// Arguments null.
        /// or
        /// Must have the same number of words as frequencies.
        /// </exception>
        public Bitmap Draw(IEnumerable<WordFrequency> wordFrequencies, Bitmap? mask = null, byte maskThreshold = 0xCF)
        {
            int fontSize = MaxFontSize;
            if (wordFrequencies == null)
            {
                throw new ArgumentNullException(nameof(wordFrequencies));
            }

            Map.Reset();
            if (mask != null)
            {
                if (mask.Width != Width || mask.Height != Height)
                {
                    throw new ArgumentException("Mask must have the same size as the word cloud");
                }

                Map.UpdateBitmapMask(mask, maskThreshold);
            }

            using (FastImage destination = new(Width, Height, PixelFormat.Format32bppArgb))
            using (Bitmap resultBitmap = destination.CreateBitmap())
            using (Graphics g = Graphics.FromImage(resultBitmap))
            {
                if (mask != null)
                {
                    if (mask.HorizontalResolution != resultBitmap.HorizontalResolution || mask.VerticalResolution != resultBitmap.VerticalResolution)
                    {
                        mask.SetResolution(resultBitmap.HorizontalResolution, resultBitmap.VerticalResolution);
                    }

                    // 创建一个新的Bitmap对象，用于存储转换后的图像
                    Bitmap newBitmap = new Bitmap(mask.Width, mask.Height, PixelFormat.Format32bppArgb);

                    // 锁定原始Bitmap的数据
                    BitmapData originalData = mask.LockBits(new Rectangle(0, 0, mask.Width, mask.Height), ImageLockMode.ReadOnly, mask.PixelFormat);

                    // 锁定新Bitmap的数据
                    BitmapData newData = newBitmap.LockBits(new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), ImageLockMode.WriteOnly, newBitmap.PixelFormat);

                    // 定义指向原始Bitmap和新Bitmap数据的指针
                    unsafe
                    {
                        byte* originalPtr = (byte*)originalData.Scan0;
                        byte* newPtr = (byte*)newData.Scan0;
                        int pixelFormatSize = Image.GetPixelFormatSize(mask.PixelFormat) / 8;

                        for (int y = 0; y < mask.Height; y++)
                        {
                            for (int x = 0; x < mask.Width; x++)
                            {
                                // 计算当前像素在数据中的索引
                                int maskIndex = (y * originalData.Stride) + (x * pixelFormatSize);
                                int newIndex = (y * newData.Stride) + (x * 4);

                                byte blue = originalPtr[maskIndex];
                                byte green = originalPtr[maskIndex + 1];
                                byte red = originalPtr[maskIndex + 2];

                                // 检查当前像素是否为白色
                                if (red == 255 || green == 255 || blue == 255)
                                {
                                    // 如果像素为白色，设置为透明颜色 (0, 0, 0, 0)
                                    newPtr[newIndex] = 0; // Blue
                                    newPtr[newIndex + 1] = 0; // Green
                                    newPtr[newIndex + 2] = 0; // Red
                                    newPtr[newIndex + 3] = 0; // Alpha
                                }
                                else
                                {
                                    // 如果像素为其它颜色，设置为黑色 (255, 0, 0, 0)
                                    newPtr[newIndex] = 0; // Blue
                                    newPtr[newIndex + 1] = 0; // Green
                                    newPtr[newIndex + 2] = 0; // Red
                                    newPtr[newIndex + 3] = 255; // Alpha
                                }
                            }
                        }
                    }

                    // 解锁Bitmap数据
                    mask.UnlockBits(originalData);
                    newBitmap.UnlockBits(newData);
                    g.DrawImage(newBitmap, 0, 0);
                }
                //resultBitmap.Save("test00.png");
                //g.Clear(Color.Transparent);
                
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                int i = 0;
                //Map.SaveDebug($"{i}.txt");
                //Map.SaveDebugImage($"{i}.png");
                Debug.WriteLine($"IsMonotonicallyIncreasing {i}: {Map.FindFirstNonIncreasingPoint()}");
                foreach (WordFrequency wordFreq in wordFrequencies)
                {
                    if (!UseRank)
                    {
                        fontSize = (int)Math.Min(fontSize, 100 * Math.Log10(wordFreq.Frequency + 100));
                    }
                    StringFormat format = StringFormat.GenericTypographic;
                    format.FormatFlags &= ~StringFormatFlags.LineLimit;

                    int posX, posY;
                    bool foundPosition = false;
                    Font font;

                    SizeF size;
                    do
                    {
                        font = new Font(FontFamily.GenericSansSerif, fontSize, GraphicsUnit.Pixel);
                        size = g.MeasureString(wordFreq.Word, font, new PointF(0, 0), format);
                        foundPosition = Map.TryFindUnoccupiedPosition((int)size.Width, (int)size.Height, out posX, out posY);
                        if (!foundPosition) fontSize -= FontStep;
                    } while (fontSize > 0 && !foundPosition);
                    if (fontSize <= 0) break;

                    g.DrawString(wordFreq.Word, font, new SolidBrush(FontColor), posX, posY, format);
                    Map.Update(destination, posX, posY);

                    i++;
                    //Map.SaveDebug($"{i}.txt");
                    //Map.SaveDebugImage($"{i}.png");
                    Debug.WriteLine($"IsMonotonicallyIncreasing {i}: {Map.FindFirstNonIncreasingPoint()}");
                }

                return new Bitmap(resultBitmap);
            }
        }


        /// <summary>
        /// Gets font colour or random if font wasn't set
        /// </summary>
        private Color FontColor
        {
            get { return m_fontColor ?? GetRandomColor(); }
            set { m_fontColor = value; }
        }

        private Color? m_fontColor;


        /// <summary>
        /// Gets a random color.
        /// </summary>
        /// <returns>Color</returns>
        private Color GetRandomColor()
        {
            return Color.FromArgb(_random.Next(0, 255), _random.Next(0, 255), _random.Next(0, 255));
        }


        /// <summary>
        /// Used to select random colors.
        /// </summary>
        private readonly Random _random;

        public int Width { get; }
        public int Height { get; }


        /// <summary>
        /// Keeps track of word positions using integral image.
        /// </summary>
        private OccupancyMap Map { get; set; }


        /// <summary>
        /// Gets or sets the maximum size of the font.
        /// </summary>
        private int MaxFontSize { get; set; }


        /// <summary>
        /// User input order instead of frequency
        /// </summary>
        private bool UseRank { get; set; }


        /// <summary>
        /// Amount to decrement font size each time a word won't fit.
        /// </summary>
        private int FontStep { get; set; }
    }

    /// <summary>
    /// Word and frequency pair
    /// </summary>
    /// <param name="Word">word ordered by occurance</param>
    /// <param name="Frequency">frequecy</param>
    public record WordFrequency(string Word, int Frequency);
}
