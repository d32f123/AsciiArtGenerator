﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace AsciiArtGenerator
{
    class AsciiArtGenerator
    {
        // Constructors--------------------------------------------------------------------------------------
        public AsciiArtGenerator(string image, double adjustment) : 
            this(image, DEFAULT_CHARS, DEFAULT_IMAGE_RES, DEFAULT_ADJUSTMENT) { }
        public AsciiArtGenerator(string image, string chars) :
            this(image, chars, DEFAULT_IMAGE_RES, DEFAULT_ADJUSTMENT) { }
        public AsciiArtGenerator(string image, int imageRes) :
            this(image, DEFAULT_CHARS, imageRes, DEFAULT_ADJUSTMENT) { }
        public AsciiArtGenerator(string image, string chars, double adjustment) :
            this(image, chars, DEFAULT_IMAGE_RES, adjustment) { }
        public AsciiArtGenerator(string image, string chars, int imageRes) :
            this(image, chars, imageRes, DEFAULT_ADJUSTMENT) { }
        public AsciiArtGenerator(string image, int imageRes, double adjustment) :
            this(image, DEFAULT_CHARS, imageRes, adjustment) { }
        public AsciiArtGenerator(string image, string chars, int imageRes, double adjustment) : this(image)
            { Chars = chars; Adjustment = adjustment; ImageRes = imageRes; }
        public AsciiArtGenerator(Parameters parameters)
        {
            if (parameters.imageFilename.Length == 0)
                throw new ArgumentNullException("No filename provided!");
            else
                ImageSource = parameters.imageFilename;

            if (parameters.chars.Length == 0)
                Chars = DEFAULT_CHARS;
            else
                Chars = parameters.chars;

            if (parameters.adjustment <= 0.0)
                Adjustment = DEFAULT_ADJUSTMENT;
            else
                Adjustment = parameters.adjustment;

            if (parameters.maxRes <= 0)
                ImageRes = DEFAULT_IMAGE_RES;
            else
                ImageRes = parameters.maxRes;
        }
        public AsciiArtGenerator(string image) { ImageSource = image; }

        // Constants (default values)------------------------------------------------------------------------
        private const string DEFAULT_CHARS = " !@#$%^&*.,+/";
        private const int DEFAULT_IMAGE_RES = 100;
        private const double DEFAULT_ADJUSTMENT = 17.0 / 8.0;


        // Settings------------------------------------------------------------------------------------------
        private int imageRes;                   // targeted image resolution (either max height or width)
                                                // the resulting art will maintain the same aspect ratio as the original
        private Bitmap bitmap;
        private string chars;                   // characters used to synthesize the resulting ASCII art
                                                // they are sorted in order of increasing brightness

        private double adjustment;              // one char is 16 pixels in height
                                                // and 9 pixels in width
                                                // therefore, to maintain aspect ratio, width should be 16.0*9.0
                                                // times greater than it actual value in the image is

        // Properties----------------------------------------------------------------------------------------
        public string ImageSource               // filename for the image to convert
        { set { bitmap = new Bitmap(value); } } // will throw an exception if file is not found
        public string Chars
        {
            get { return (string)chars.Clone(); }
            set { chars = CharBrightness.GetSortedCharacters(value); }
        }
        public double Adjustment { get { return adjustment; } set { adjustment = value; } }
        public int ImageRes { get { return imageRes; } set { imageRes = value; } }

        // Methods-------------------------------------------------------------------------------------------

        /** 
         *  <summary>Resizes an image to the given width and height</summary>
         *  <param name="height">The resulting height of the new image</param>
         *  <param name="width">The resulting width of the new image</param>
         *  <param name="image">The image to be transformed</param>
         *  <returns>Returns source image with dimensions of width and height</returns>
         **/
        private Bitmap Resize(Bitmap image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /**
         * <summary>Returns dimensions that maintain the same aspect ratio as the original image
         * and that are adjusted according to the differences in character height and width</summary>
         * <param name="newHeight">The wanna-be height of the resulting image(in pixels)</param>
         * <param name="newWidth">The wanna-be width of the resulting image(in pixels)</param>
         **/
        public Size getSizeWithAdjustment(Bitmap source, int newWidth, int newHeight)
        {
            var ratioX = (double)newWidth / source.Width;
            var ratioY = (double)newHeight / source.Height;
            var ratio = Math.Min(ratioX, ratioY);

            return new Size((int)(source.Width * ratio), (int)(source.Height * ratio / adjustment));
        }

        /**
         * <summary>Does the whole job of converting an image pointed to by its' filename to ASCII art</summary>
         * <param name="filename">The filename of the image to be converted</param>
         **/
        public void Convert(string filename)
        {
            Size newSize = getSizeWithAdjustment(bitmap, imageRes, imageRes);
            using (var dest = new StreamWriter(filename))
            {
                Bitmap bitmap = Resize(this.bitmap, newSize.Width, newSize.Height);
                for (var y = 0; y < bitmap.Height; ++y)
                {
                    for (var x = 0; x < bitmap.Width; ++x)
                    {
                        var color = bitmap.GetPixel(x, y);
                        var brightness = 1 - Brightness(color) / 255.0;

                        int charIndex = (int)Math.Round((chars.Length - 1) * brightness);
                        char ch = chars[charIndex];

                        dest.Write(ch);
                    }
                    dest.WriteLine();
                }
            }
        }
        
        /**
         * <summary>Returns a brighness of a given color</summary>
         * <param name="c">Color to be analyzed</param>
         * <returns>Brightness of the given color</returns>
         */
        private double Brightness(Color c)
        {
            return Math.Sqrt(Math.Pow(c.R, 2) * .241 +
                                Math.Pow(c.G, 2) * .691 +
                                Math.Pow(c.B, 2) * .068);
        }
    }
}