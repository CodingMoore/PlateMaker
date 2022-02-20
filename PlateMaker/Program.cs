
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV.Features2D;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System.Text.RegularExpressions;

namespace PlateScanner
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Program Started");

            // Defines the directory in which to look for the plate image files
            string imageImportDirectory = "C:\\Users\\Randel\\source\\repos\\PlateScanner\\PlateScanner\\Images\\";

            // Creates an array of all files found in the specified directory that contain the specified value.
            string[] files = { };
            files = Directory.GetFiles($"{imageImportDirectory}", "*.jpg", SearchOption.AllDirectories);

            // removes the last directory from the image Import file path.  This will be use later to save files in a sibling directory
            string fileSavingDirectoryStart = Regex.Replace(imageImportDirectory, @"[^\\]+\\?$", "");

            // For each file in the array....
            foreach (string file in files)
            {
                Console.WriteLine(file);

                // Creates a new plate object for the individual image file
                Plate plate = new Plate(file, fileSavingDirectoryStart);

                // noise suppression
                //Image<Bgr, byte> smoothedPlateImage = plate.NoiseSuppression();

                // Get the coordinates of the center of the plate based on red colored blob/shape
                plate.GetPlateCenter(plate.PlateImage);

                // Get the coordinates of the top of the plate based on a green colored blob/shape
                plate.GetPlateTopPoint(plate.PlateImage);

                // Translate position of plate so that its center matches that of the image center
                Image<Bgr, byte> translatedPlateImage = plate.TranslatePlateCenter(plate.PlateImage);

                // Rotates the plate so that it is perfectly vertical (based on the position of the red(center) and green(top) blobs/shapes
                Image<Bgr, byte> rotatedPlateImage = plate.RotatePlatetoVertical(translatedPlateImage);

                // Because the rotation algorithm blurs the contours/shapes, creating grey squares, we need to get the image back to being binary.
                Image<Gray, byte> binaryPlateImage = plate.MakeImageBinary(rotatedPlateImage);

                // Creates a plate image where all blobs/shapes are selected
                List<int[]> centerAndRadius = plate.FindContoursCenterAndRadius(binaryPlateImage);

                // Creates SVG file from contours (and saves it to disk), and returns the svg as a string so we can make an Html file with it.
                string svgString = plate.CreateSvgFromPhoto(centerAndRadius);

                // Creates an Html file based on the htmlString
                plate.CreateHtml(svgString);

                // Display the plate image
                //ImageViewer viewer = new ImageViewer(binaryAllContoursPlateMat);
                //viewer.Image = binaryAllContoursPlateMat;
                //viewer.ShowDialog();

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