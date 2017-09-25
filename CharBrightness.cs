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

        public struct CharWithBrightness
        {
            public double brightness;
            public char ch;
        }
        
        // A comparer class for sorting characters according to their brightness
        private class CharWithBrightnessComparer : Comparer<CharWithBrightness>
        {
            private const int FACTOR = 10000; 
            public override int Compare(CharWithBrightness x, CharWithBrightness y)
            {
                return (int)Math.Round(x.brightness * FACTOR - y.brightness * FACTOR);
            }
        }

        /**
         * <summary>This method is a proxy for <see cref="GetSortedCharacters(string)"/> 
         *      that compiles a string of ascii characters from 32 to 127</summary>
         **/
        public static List<CharWithBrightness> GetSortedCharacters()
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
        public static List<CharWithBrightness> GetSortedCharacters(string chars)
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

            list.Sort(new CharWithBrightnessComparer());



            return NormalizeBrightness(RemoveDuplicates(list));
        }


        /**
         * <summary>Returns a normalized list (values from 0.0 to 1.0) based on a SORTED list</summary>
         * <param name="list">List to be normalized</param>
         * <returns>A normalized list</returns>
         **/ 
        private static List<CharWithBrightness> NormalizeBrightness(List<CharWithBrightness> list)
        {
            var max = list[list.Count - 1].brightness;

            List<CharWithBrightness> ret = new List<CharWithBrightness>(list.Count);

            foreach (CharWithBrightness curVal in list)
                ret.Add(new CharWithBrightness { brightness = curVal.brightness / max, ch = curVal.ch });

            return ret;
        }

        /**
         * <summary>Returns a list without duplicates based on a SORTED list</summary>
         * <returns>A list without duplicates</returns>
         **/
        private static List<CharWithBrightness> RemoveDuplicates(List<CharWithBrightness> list)
        {
            List<CharWithBrightness> ret = new List<CharWithBrightness>(list.Count);

            var prev = list[0].brightness;
            ret.Add(list[0]);
            for (var i = 1; i < list.Count; ++i)
            {
                if (list[i].brightness == prev)
                    continue;
                prev = list[i].brightness;
                ret.Add(list[i]);
            }

            return ret;
        }

        private static double Brightness(Color c)
        {
            return Math.Sqrt(Math.Pow(c.R, 2) * .241 +
                                Math.Pow(c.G, 2) * .691 +
                                Math.Pow(c.B, 2) * .068);
        }
    }
}
