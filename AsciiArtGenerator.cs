using System;
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

            if (parameters.invert != DEFAULT_INVERT_COLOR)
                invertColor = true;
            else
                invertColor = DEFAULT_INVERT_COLOR;
        }
        public AsciiArtGenerator(string image) { ImageSource = image; }

        // Constants (default values)------------------------------------------------------------------------
        private const string DEFAULT_CHARS = "  ~`!12@3#4$5%6^7&8*9(0)-_=+}]{[|:;/\\\"'?.,MmnNbBvVcCxXzZlLkKjJhHgGfFdDsSaAoOpPiIuUyYtTrReEwWqQ";
        private const int DEFAULT_IMAGE_RES = 100;
        private const double DEFAULT_ADJUSTMENT = 17.0 / 8.0;
        private const bool DEFAULT_INVERT_COLOR = false;


        // Settings------------------------------------------------------------------------------------------
        private int imageRes;                   // targeted image resolution (either max height or width)
                                                // the resulting art will maintain the same aspect ratio as the original
        private Bitmap source;
        private List<CharBrightness.CharWithBrightness> chars;                   
                                                // characters used to synthesize the resulting ASCII art
                                                // they are sorted in order of increasing brightness

        private double adjustment;              // one char is 16 pixels in height
                                                // and 9 pixels in width
                                                // therefore, to maintain aspect ratio, width should be 16.0*9.0
                                                // times greater than it actual value in the image is

        private bool invertColor;               // Set this to true to invert the resulting color

        // Properties----------------------------------------------------------------------------------------
        public string ImageSource               // filename for the image to convert
        { set { source = new Bitmap(value); } } // will throw an exception if file is not found
        public string Chars
        { set { chars = CharBrightness.GetSortedCharacters(value); } }
        public double Adjustment { get { return adjustment; } set { adjustment = value; } }
        public int ImageRes { get { return imageRes; } set { imageRes = value; } }
        public bool InvertColor { get { return invertColor; } set { invertColor = value; } }

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
        public void Convert(TextWriter dest)
        {
            // Get the resulting image dimensions
            Size newSize = getSizeWithAdjustment(source, imageRes, imageRes);

            // Create the resulting image (ready for conversion)
            Bitmap bitmap = Resize(this.source, newSize.Width, newSize.Height);
            for (var y = 0; y < bitmap.Height; ++y)
            {
                for (var x = 0; x < bitmap.Width; ++x)
                {
                    // For every pixel check its' brightness
                    var color = bitmap.GetPixel(x, y);
                    // invert the brightness if InvertColor is set
                    // this value varies from 0.0 to 1.0
                    var brightness = Brightness(color) / 255.0;
                    if (invertColor)
                        brightness = 1 - brightness;

                    // And select a character based on its' brightness
                    char ch = GetClosestChar(chars, brightness);

                    dest.Write(ch);
                }
                dest.WriteLine();
            }
        }

        /**
         * <summary>Performs a closest neighbor search for the given list
         * Uses binary search</summary>
         * <param name="brightness">The wanna-be brightness of the output character</param>
         * <param name="list">The list of characters and their corresponding brightness levels</param>
         * <returns>Character with brightness being closest to the given parameter</returns>
         **/
        public char GetClosestChar(List<CharBrightness.CharWithBrightness> list, double brightness)
        {
            if (list.Count == 1)
                return list[0].ch;
            int high = list.Count - 1, low = 0;
            int med;
            do
            {
                med = (high + low) / 2;
                if (list[med].brightness == brightness)
                    return list[med].ch;
                if (brightness < list[med].brightness)
                    high = med;
                else
                    low = med;
            } while (high - low > 1);

            if (Math.Abs(brightness - list[low].brightness) < Math.Abs(brightness - list[high].brightness))
                return list[low].ch;
            else
                return list[high].ch;
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
