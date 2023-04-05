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
                //if (mask != null)
                //{
                //    if (mask.HorizontalResolution != resultBitmap.HorizontalResolution || mask.VerticalResolution != resultBitmap.VerticalResolution)
                //    {
                //        mask.SetResolution(resultBitmap.HorizontalResolution, resultBitmap.VerticalResolution);
                //    }
                //    g.DrawImage(mask, 0, 0);
                //}
                g.Clear(Color.Transparent);
                
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                int i = 0;
                Map.SaveDebug($"{i}.txt");
                Map.SaveDebugImage($"{i}.png");
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

                    //using (Bitmap tmpBitmap = new((int)size.Width, (int)size.Height, PixelFormat.Format32bppArgb))
                    //{
                    //    using (Graphics tmpG = Graphics.FromImage(tmpBitmap))
                    //    {
                    //        tmpG.Clear(Color.Transparent);
                    //        tmpG.TextRenderingHint = TextRenderingHint.AntiAlias;
                    //        tmpG.DrawString(wordFreq.Word, font, new SolidBrush(FontColor), 0, 0, format);
                    //        Map.Update(tmpBitmap, new Rectangle(posX, posY, tmpBitmap.Width, tmpBitmap.Height));
                    //    }

                    //    g.DrawRectangle(new Pen(new SolidBrush(FontColor)), new Rectangle(posX, posY, tmpBitmap.Width, tmpBitmap.Height));
                    //    g.DrawImage(tmpBitmap, posX, posY);
                    //    i += 1;
                    //    Map.SaveDebug($"{i}.txt");
                    //    Map.SaveDebugImage($"{i}.png");
                    //    Debug.WriteLine($"IsMonotonicallyIncreasing {i}: {Map.FindFirstNonIncreasingPoint()}");
                    //}

                    g.DrawString(wordFreq.Word, font, new SolidBrush(FontColor), posX, posY, format);
                    Map.Update(destination, posX, posY);

                    i++;
                    Map.SaveDebug($"{i}.txt");
                    Map.SaveDebugImage($"{i}.png");
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
