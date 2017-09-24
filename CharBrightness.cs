using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsciiArtGenerator
{
    // This class provides functionality to 
    // check the brighness of the individual characters
    // and then sort them based on the brightness
    static class CharBrightness
    {
        private const int DEFAULT_WIDTH = 100;
        private const int DEFAULT_HEIGHT = 100;

        private struct CharWithBrightness
        {
            public double brightness;
            public char ch;
        }

        private class CharWithBrighnessComparer : Comparer<CharWithBrightness>
        {
            public override int Compare(CharWithBrightness x, CharWithBrightness y)
            {
                return (int)Math.Round(x.brightness - y.brightness);
            }
        }

        public static string GetSortedCharacters()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (char i = (char)32; i < (char)127; ++i)
                stringBuilder.Append(i);
            return GetSortedCharacters(stringBuilder.ToString());
        }

        public static string GetSortedCharacters(string chars)
        {
            List<CharWithBrightness> list = new List<CharWithBrightness>(chars.Length);

            Font drawFont = SystemFonts.DefaultFont;
            SolidBrush drawBrush = new SolidBrush(Color.White);
            PointF drawPoint = new PointF(0.0F, 0.0F);

            int height = DEFAULT_HEIGHT, width = DEFAULT_WIDTH;

            Bitmap bitmap = new Bitmap(DEFAULT_WIDTH, DEFAULT_HEIGHT); // just a random bitmap size
            Graphics graphics = Graphics.FromImage(bitmap);

            
            for (int i = 0; i < chars.Length; ++i)
            {
                string drawString = "";
                drawString += chars[i];

                SizeF size = graphics.MeasureString(drawString, drawFont);
                PointF rect = new PointF(size.Width, size.Height);

                width = (int)Math.Ceiling(rect.X);
                height = (int)Math.Ceiling(rect.Y);
                bitmap = new Bitmap(width, height);
                graphics = Graphics.FromImage(bitmap);

                graphics.Clear(Color.Black);
                graphics.DrawString(drawString, drawFont, drawBrush, drawPoint);

                double sum = 0.0; // the sum of all pixel brightnesses
                for (var y = 0; y < bitmap.Height; ++y)
                {
                    for (var x = 0; x < bitmap.Width; ++x)
                    {
                        Color color = bitmap.GetPixel(x, y);
                        sum += Brightness(color);
                    }
                }

                list.Add(new CharWithBrightness { brightness = sum / (width * height), ch = chars[i] });
            }

            list.Sort(new CharWithBrighnessComparer());

            StringBuilder result = new StringBuilder(chars.Length);
            for (int i = 0; i < chars.Length; ++i)
                result.Append(list[i].ch);
            return result.ToString();
        }

        private static double Brightness(Color c)
        {
            return Math.Sqrt(Math.Pow(c.R, 2) * .241 +
                                Math.Pow(c.G, 2) * .691 +
                                Math.Pow(c.B, 2) * .068);
        }
    }
}
