using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV.Features2D;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;


namespace PlateMaker
{
    class Plate
    {
        public string FileName { get; set; }
        public FileInfo SvgFilePath { get; set; }
        public FileInfo HtmlFilePath { get; set; }

        public Plate(string file, string fileSavingDirectoryStart)
        {
            // Extracts the original Image file name from the full file path, and removes the file extension.
            // This will be used later when we save each image's svg and html files as they will use the same file name.
            FileName = Path.GetFileNameWithoutExtension(file);

            // Defines the full directory path for saving the file
            SvgFilePath = new FileInfo($"{fileSavingDirectoryStart}svg Files\\");

            // Defines the full directory path for saving the file
            HtmlFilePath = new FileInfo($"{fileSavingDirectoryStart}html Files\\");

        }

        public string CreateSvgFromApiCoordinates(Dictionary<int, string[]> stellarObjectData)
        {
            // Creates a new StringBuilder object
            var svgStringBuilder = new StringBuilder();

            // Creates the a scaling multiplier to adjust the zoom of the object image on the sdss website
            string photoScaler = ".5";

            // Creates a Scaling Multiplier to adjust the viewbox size while keeping the location of the svg contents scaled to match.
            int plateScalingMultiplier = 2;

            // Creates the radius of the dots on the plate.  The ratio of this number to the plateScalingMulitplier affect the proportional dot size.
            int svgDotRadius = 2;

            // Creates Viewbox Size variables
            int xViewBoxMin = 0 * plateScalingMultiplier;
            int yViewBoxMin = 0 * plateScalingMultiplier;
            int xViewBoxMax = 800 * plateScalingMultiplier;
            int yViewBoxMax = 800 * plateScalingMultiplier;

            // Creates PlateEdge Size variables
            int xSvgPlateCenter = xViewBoxMax / 2;
            int ySvgPlateCenter = yViewBoxMax / 2;
            int SvgPlateRadius = 394 * plateScalingMultiplier;

            // Creates our opening svg string to be tacked on to the beginning of the Stringbuilder
            string svgOpen = $"<svg id='svgImage' width='100vw' height='100vh' viewBox='{xViewBoxMin} {yViewBoxMin} {xViewBoxMax} {yViewBoxMax}' transform-origin='0 0'>";

            // Creates the plate border to be added to the Stringbuilder
            string svgPlateEdge = $"<circle cx='{xSvgPlateCenter}' cy='{ySvgPlateCenter}' r='{SvgPlateRadius}' strok='grey' stroke-width='1' />";

            // Creates our closing svg string to be tacked on to the end of the Stringbuilder
            string svgClose = "</svg>";

            // Adds the opening SVG string to the Stringbuilder
            svgStringBuilder.Append(svgOpen);
            svgStringBuilder.Append(svgPlateEdge);


            // for every center and radius in our list, create a sub string to be use the the svg file
            for (int i = 0; i < stellarObjectData.Count; i++)
            {
                double cxScaledAndTranslatedInt = double.Parse(stellarObjectData[i][0]) * plateScalingMultiplier + 400 * plateScalingMultiplier;
                double cyScaledAndTranslatedInt = double.Parse(stellarObjectData[i][1]) * plateScalingMultiplier + 400 * plateScalingMultiplier;

                string cxScaledAndTranslatedString = cxScaledAndTranslatedInt.ToString();
                string cyScaledAndTranslatedString = cyScaledAndTranslatedInt.ToString();

                svgStringBuilder.Append(
                    // Ampersands "&" in the href query string have been replaced with "&amp;" since a regular Ampersand is a escapement character in XML (svg).
                    $"<a href='https://skyserver.sdss.org/dr17/VisualTools/navi?ra={stellarObjectData[i][4]}&amp;dec={stellarObjectData[i][5]}&amp;scale={photoScaler}' target='_blank'> " +
                    $"<circle cx='{cxScaledAndTranslatedString}' cy='{cyScaledAndTranslatedString}' r='{svgDotRadius}' stroke='black' stroke-width='1' fill='red'/>" +
                    $"{stellarObjectData[i][2]}, plate: {stellarObjectData[i][3]}</a>"
                );
            }

            // Adds the closing SVG strign to the String Builder
            svgStringBuilder.Append(svgClose);

            // Converts the Stringbuilder to a string
            string svgString = svgStringBuilder.ToString();

            // Creates the file directory if the directory does not already exist.  If the directory does already exist, this method does nothing.
            SvgFilePath.Directory.Create();

            // Writes the svg file to disk based on the svgString
            File.WriteAllText($"{SvgFilePath}{FileName}.svg", svgString);

            return svgString;
        }

        public void CreateHtmlFromSvgFromApi(string svgStringFromApi)
        {
            // Creates a new StringBuilder object
            StringBuilder htmlStringBuilder = new StringBuilder();

            // Creates our opening and closing html strings to be tacked on to the Stringbuilder
            string htmlOpen = "" +
                "<!DOCTYPE html>" +
                "<html lang='en'>" +
                "<head>" +
                    "<meta charset='UTF-8'>" +
                    "<meta http-equiv='X-UA-Compatible' content='IE=edge'>" +
                    "<meta name='viewport' content='width=device-width, initial-scale=1.0'>" +
                    "<title>Document</title>" +
                    "<script src='https://www.unpkg.com/@panzoom/panzoom/dist/panzoom.js'></script>" +
                "</head>" +
                "<body>" +
                    "<div id='svgWrapper' style='display: flex; justify-content: center; align-items: center'>" +
                 "";

            string htmlClose = "" +
                    "</div>" +
                    "<script> " +
                        "const element = document.getElementById('svgWrapper')," +
                        // the below reads like it has a "const" at the beginning because of the comma from the above line.
                        "panzoom = Panzoom(element, {" +
                        // options here
                        "maxScale: 50," +
                        "minScale: .75" +
                        "});" +
                        // enable mouse wheel
                        "const parent = element.parentElement;" +
                        "parent.addEventListener('wheel', panzoom.zoomWithWheel);" +
                    "</script>" +
                "</body>" +
                "</html>" +
                "";

            // Adds the opening html string, then the svgString, and finally the closing html string to the Stringbuilder
            htmlStringBuilder.Append(htmlOpen);
            htmlStringBuilder.Append(svgStringFromApi);
            htmlStringBuilder.Append(htmlClose);

            // Converts the Stringbuilder to a string
            string htmlString = htmlStringBuilder.ToString();

            // Creates the file directory if the directory does not already exist.  If the directory does already exist, this method does nothing.
            HtmlFilePath.Directory.Create();

            // Writes the html file to disk based on the htmlString, which in turn is based partially on the svgString
            File.WriteAllText($"{HtmlFilePath}{FileName}.html", htmlString);

        }

    }
}