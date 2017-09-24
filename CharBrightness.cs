using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsciiArtGenerator
{
    /** 
     * <summary>This class provides functionality to 
     * check the brighness of the individual characters
     * and then sort them based on the brightness</summary>
     **/
    static class CharBrightness
    {

        //Default width and height of the area on which to draw individual characters (see GetSortedCharacters method)
        private const int DEFAULT_WIDTH = 100;
        private const int DEFAULT_HEIGHT = 100;

        private struct CharWithBrightness
        {
            public double brightness;
            public char ch;
        }
        
        // A comparer class for sorting characters according to their brightness
        private class CharWithBrighnessComparer : Comparer<CharWithBrightness>
        {
            public override int Compare(CharWithBrightness x, CharWithBrightness y)
            {
                return (int)Math.Round(x.brightness - y.brightness);
            }
        }

        /**
         * <summary>This method is a proxy for <see cref="GetSortedCharacters(string)"/> 
         *      that compiles a string of ascii characters from 32 to 127</summary>
         **/
        public static string GetSortedCharacters()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (char i = (char)32; i < (char)127; ++i)
                stringBuilder.Append(i);
            return GetSortedCharacters(stringBuilder.ToString());
        }

        /**
         * <summary>This method sorts a string of chars based on their brightness</summary>
         * <example>string sortedChars = GetSortedCharacters(" .-_+=")</example>
         * <param name="chars">Characters to sort</param>
         * <returns>Sorted characters based on their brightness</returns>
         **/
        public static string GetSortedCharacters(string chars)
        {
            List<CharWithBrightness> list = new List<CharWithBrightness>(chars.Length);

            // set font, brush, and the point from which the character will be drawn
            Font drawFont = SystemFonts.DefaultFont;
            SolidBrush drawBrush = new SolidBrush(Color.White);
            PointF drawPoint = new PointF(0.0F, 0.0F);

            // set the default height and width of the area on which to draw
            int height = DEFAULT_HEIGHT, width = DEFAULT_WIDTH;

            // create the bitmap and the graphics surface
            Bitmap bitmap = new Bitmap(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            Graphics graphics = Graphics.FromImage(bitmap);

            // for every character
            for (int i = 0; i < chars.Length; ++i)
            {
                string drawString = "";
                drawString += chars[i];

                // check its' size in pixels
                SizeF size = graphics.MeasureString(drawString, drawFont);
                PointF rect = new PointF(size.Width, size.Height);

                // generate a new surface on which to draw based on the size of the character
                width = (int)Math.Ceiling(rect.X);
                height = (int)Math.Ceiling(rect.Y);
                bitmap = new Bitmap(width, height);
                graphics = Graphics.FromImage(bitmap);

                // fill the area completely black
                graphics.Clear(Color.Black);
                // draw the character
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

                // get the average value of brightness (sum / (width * height)) and add the character to the list
                list.Add(new CharWithBrightness { brightness = sum / (width * height), ch = chars[i] });
            }

            list.Sort(new CharWithBrighnessComparer());

            // return the result, converting from list to string
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
