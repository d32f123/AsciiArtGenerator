using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* This program converts an image of any known format
 * to ASCII charset art based on the brightness of 
 * every individual pixel.
 * 
 * For usage, try running the program without arguments
 */

namespace AsciiArtGenerator
{
    struct Parameters
    {
        public string imageFilename;
        public string textFilename;
        public double adjustment;
        public string chars;
        public int maxRes;
        public bool invert;
    }

    class Program
    {
        private static string usage =
            "Usage: AsciiArtGenerator <filename> [-o output_file] [-a adjustment] [-c chars] [-r max_res] [-i]\n" +
            "\t-o output_file: resulting file,\n\t-a adjustment: char height to width ratio\n\t" +
            "-c chars: characters used in building ascii art\n\t-r tmax_res: max resolution of the resulting image\n" +
            "-i: invert the resulting image";

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.Write(usage);
                Console.ReadKey();
                return;
            }

            Parameters parameters = GetParameters(args);

            var artGenerator = new AsciiArtGenerator(parameters);
            artGenerator.Convert(parameters.textFilename);
        }

        // Gets parameters from console
        // For more info, see usage
        static Parameters GetParameters(string[] args)
        {
            Parameters parameters = new Parameters();

            // first parameter should be filename
            parameters.imageFilename = args[0];
            // set the defaults
            parameters.adjustment = -1.0;
            parameters.chars = "";
            parameters.textFilename = "ascii.txt";
            parameters.maxRes = -1;
            parameters.invert = false;

            for (int i = 1; i < args.Length; ++i)
            {
                string key = args[i++];
                if (key == "-o")
                {
                    parameters.textFilename = args[i];
                }
                else if (key == "-a")
                {
                    try
                    {
                        parameters.adjustment = Convert.ToDouble(args[i]);
                    }
                    catch (Exception)
                    {
                        Console.Error.WriteLine("Adjustment should be a double value.. Using default\n");
                    }
                }
                else if (key == "-c")
                {
                    parameters.chars = args[i];
                }
                else if (key == "-r")
                {
                    try
                    {
                        parameters.maxRes = Convert.ToInt32(args[i]);
                    }
                    catch (Exception)
                    {
                        Console.Error.WriteLine("max_res should be an integer value.. Using default\n");
                    }
                }
                else if (key == "-i")
                {
                    --i;                    // no second argument for this key
                    parameters.invert = true;
                }
                else                        // unknown key
                    continue;
            }

            return parameters;
        }
    }
}
