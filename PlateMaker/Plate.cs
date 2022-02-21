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

            ////////////////////////////////
            ///// Adjustable Constants /////
            ////////////////////////////////
    
            // Creates the a scaling multiplier to adjust the zoom of the object image on the sdss website
            string photoScaler = ".5"; 

            // The dotScaler affects distance between the dots since it is multiplied by each dot's x and y coordinates.
            // Since the area of the dots is automatically scaled to fit its container, this has the effect of changing the perceived dot size.
            // The HIGHER the number, the SMALLER the dots (.5 is larger than 3)
            double dotScaler = 1;

            // Creates a Relative size multiplier for the plate dot area in relation to the plate border.
            // If the multiplier was set at 1, the dots would completely fill the plate from border to border.
            double dotAreaScaler = .8;

            // Creates a scaling multiplier to define the width of the stroke in relation to the size of the dot.
            // If you change the dotScaler, the stroke width will scale with it.
            // If you change the strokeWidthScaler, then you change the stroke width relative to the dot size.
            double strokeWidthScaler = .5;

            ////////////////////////////////
            ////////////////////////////////



            // The stroke width of the dots, as calculated from the constants above.
            double strokeWidth = strokeWidthScaler * dotAreaScaler;

            // Creates a compound scaler used to calculate how the dots are laid out on the screen. Calculated from the constants above.
            double relativeSizeScaler = dotAreaScaler * dotScaler;

            // Creates Viewbox Size constants. You should probably leave these alone.
            // If you want to change the relative size of the area that the dots take up, change the "dotAreaScaler" instead.
            double xViewBoxMin = 0 * dotScaler;
            double yViewBoxMin = 0 * dotScaler;
            double xViewBoxMax = 800 * dotScaler;
            double yViewBoxMax = 800 * dotScaler;

            // Creates our opening svg string to be tacked on to the beginning of the Stringbuilder
            string svgOpenOuter = $"<svg id='svgImage' width='100vw' height='100vh' viewBox='{xViewBoxMin} {yViewBoxMin} {xViewBoxMax} {yViewBoxMax}' transform-origin='0 0'>";
            // Creates our closing svg string to be tacked on to the end of the Stringbuilder
            string svgCloseOuter = "</svg>";

            // Creates the plate border to be added to the Stringbuilder
            // string svgPlateEdge = $"<circle cx='{xSvgPlateCenter}' cy='{ySvgPlateCenter}' r='{SvgPlateRadius}' strok='grey' stroke-width='1' />";

            string svgPlateEdge = $"<svg viewBox='0 0 120 120' transform-origin='0 0' style='fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;'>" +
                "<path d='M58.526,10.597L58.526,9.266C58.526,9.172 58.601,9.095 58.696," +
                "9.092C59.129,9.081 59.564,9.076 60,9.076C60.436,9.076 60.871,9.081 61.304,9.092C61.399,9.095 61.474,9.172 61.474," +
                "9.266L61.474,10.597C88.071,11.377 109.424,33.215 109.424,60C109.424,87.278 87.278,109.424 60,109.424C32.722," +
                "109.424 10.576,87.278 10.576,60C10.576,33.215 31.929,11.377 58.526,10.597ZM61.3,9.266L61.3,10.767C87.882," +
                "11.457 109.25,33.253 109.25,60C109.25,87.182 87.182,109.25 60,109.25C32.818,109.25 10.75,87.182 10.75,60C10.75," +
                "33.253 32.118,11.457 58.7,10.767L58.7,9.266C59.132,9.255 59.565,9.25 60,9.25C60.435,9.25 60.868,9.255 61.3,9.266Z'" +
                " />" +
                "</svg>";

            // Creates a new StringBuilder object
            var svgStringBuilder = new StringBuilder();

            // Adds the opening SVG string to the Stringbuilder
            svgStringBuilder.Append(svgOpenOuter);
            svgStringBuilder.Append(svgPlateEdge);


            // for every center and radius in our list, create a sub string to be use the the svg file
            for (int i = 0; i < stellarObjectData.Count; i++)
            {
                // When the stellarObjectData is multiplied by the relativeSizeScaler, we change the size of the overall area of the dots.
                // When we add teh xViewBoxMax/2 and yViewBoxMax/2 value, we are translating each dot's x,y coordinates so that they are all
                // in the positive x and positive y coordinate quadrant, which places them in the viewbox.
                double cxScaledAndTranslatedInt = double.Parse(stellarObjectData[i][0]) * relativeSizeScaler + xViewBoxMax / 2;
                double cyScaledAndTranslatedInt = double.Parse(stellarObjectData[i][1]) * relativeSizeScaler + yViewBoxMax / 2;

                string cxScaledAndTranslatedString = cxScaledAndTranslatedInt.ToString();
                string cyScaledAndTranslatedString = cyScaledAndTranslatedInt.ToString();

                svgStringBuilder.Append(
                    // Ampersands "&" in the href query string have been replaced with "&amp;" since a regular Ampersand is a escapement character in XML (svg).
                    $"<a href='https://skyserver.sdss.org/dr17/VisualTools/navi?ra={stellarObjectData[i][4]}&amp;dec={stellarObjectData[i][5]}&amp;scale={photoScaler}' target='_blank'> " +
                    $"<circle cx='{cxScaledAndTranslatedString}' cy='{cyScaledAndTranslatedString}' r='{dotAreaScaler}' stroke='black' stroke-width='{strokeWidth}' fill='red'/>" +
                    $"{stellarObjectData[i][2]}, plate: {stellarObjectData[i][3]}</a>"
                );
            }

            // Adds the closing SVG strign to the String Builder
            svgStringBuilder.Append(svgCloseOuter);

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