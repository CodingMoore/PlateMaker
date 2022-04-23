
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV.Features2D;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System.Text.RegularExpressions;
using CsvHelper;
using System.Globalization;

namespace PlateMaker
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Program Started");


            // Defines the directory in which to look for the plate image files
            string csvImportDirectory = "C:\\Users\\Randel\\source\\repos\\PlateMaker\\PlateMaker\\Images\\";

            var reader = new StreamReader(csvImportDirectory);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Creates an array of all files found in the specified directory that contain the specified value.
            string[] files = { };
            files = Directory.GetFiles($"{csvImportDirectory}", "*.jpg", SearchOption.AllDirectories);

            // removes the last directory from the image Import file path.  This will be use later to save files in a sibling directory
            string fileSavingDirectoryStart = Regex.Replace(csvImportDirectory, @"[^\\]+\\?$", "");

            // For each file in the array....
            foreach (string file in files)
            {
                Console.WriteLine(file);

                // Creates a new plate object for the individual image file
                Plate plate = new Plate(file, fileSavingDirectoryStart);

                // pulls the plate number from the file name, since every file should be named by its number.
                string plateNumber = plate.FileName;

                // creates an apiCallObject instance.
                ApiCallObject apiCall = new ApiCallObject(plateNumber);

                // Makes the API call
                Dictionary<int, string[]> stellarObjectData = apiCall.MakeTheApiCall();

                string svgStringFromApi = plate.CreateSvgFromApiCoordinates(stellarObjectData);

                plate.CreateHtmlFromSvgFromApi(svgStringFromApi);


                Console.WriteLine("Plate " + plateNumber + " " + "has been completed");
            }

            Console.WriteLine("Program Finished");

            Console.ReadKey();
        }

    }
}