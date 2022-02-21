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


            // Styling constants
            string plateBorderStyle = "style='stroke:rgb(46,46,46); stroke-width:1%;'";

            string plateFillStyle = "style='fill:rgb(92,92,92);'";

            string plateEighthInchCenterHoleStyle = "style='fill:rgb(76,0,255);'";

            string plateQuarterInchHoleStyle = "style='fill:rgb(255,0,0);'";

            string plateHalfInch180DegreeSpacedHoleStyle = "style='fill:rgb(0,181,255);'";

            string plateHalfInch30DegreeSpacedHoleStyle = "style='fill:rgb(102,255,0);'";

            // default colors for the dot stroke and fill
            string dotStrokeColor = "rgb(255, 255, 255)";
            string dotFillColor = "rgb(255, 255, 255)";

            // GALAXY - colors for dot stroke and fill
            string galaxyStrokeColor = "rgb(255, 0, 132)";
            string galaxyFillColor = "rgb(255, 255, 255)";

            // STAR - colors for dot stroke and fill
            string starStrokeColor = "rgb(0,181,255)";
            string starFillColor = "rgb(255, 255, 255)";

            // QSO - colors for dot stroke and fill
            string qsoStrokeColor = "rgb(102,255,0)";
            string qsoFillColor = "rgb(255, 255, 255)";

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

            // Creates the plate border, which will be added to the Stringbuilder.
            string svgPlateBorder = 
                "<svg viewBox='0 0 120 120' style='fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;'>" +
                    "<path d='M58.526,10.597L58.526,9.266C58.526,9.172 58.601,9.095 58.696," +
                    "9.092C59.129,9.081 59.564,9.076 60,9.076C60.436,9.076 60.871,9.081 61.304,9.092C61.399,9.095 61.474,9.172 61.474," +
                    "9.266L61.474,10.597C88.071,11.377 109.424,33.215 109.424,60C109.424,87.278 87.278,109.424 60,109.424C32.722," +
                    "109.424 10.576,87.278 10.576,60C10.576,33.215 31.929,11.377 58.526,10.597ZM61.3,9.266L61.3,10.767C87.882," +
                    "11.457 109.25,33.253 109.25,60C109.25,87.182 87.182,109.25 60,109.25C32.818,109.25 10.75,87.182 10.75,60C10.75," +
                    "33.253 32.118,11.457 58.7,10.767L58.7,9.266C59.132,9.255 59.565,9.25 60,9.25C60.435,9.25 60.868,9.255 61.3,9.266Z'" +
                    $"{plateBorderStyle}/>" +
                "</svg>";

            // Creates the plate fill, which will be added to the Stringbuilder
            string svgPlateFill =
                "<svg viewBox='0 0 120 120' style='fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;'>" +
                    "<path d='M61.3,9.266L61.3,10.767C87.882,11.457 109.25,33.253 109.25,60C109.25,87.182 87.182,109.25 60,109.25C32.818," +
                    "109.25 10.75,87.182 10.75,60C10.75,33.253 32.118,11.457 58.7,10.767L58.7,9.266C59.132,9.255 59.565,9.25 60,9.25C60.435," +
                    "9.25 60.868,9.255 61.3,9.266ZM71.802,104.047C71.376,104.161 70.937,103.908 70.823,103.481C70.708,103.055 70.962," +
                    "102.616 71.388,102.501C71.815,102.387 72.254,102.641 72.368,103.067C72.482,103.494 72.229,103.933 71.802,104.047ZM48.198," +
                    "104.047C47.771,103.933 47.518,103.494 47.632,103.067C47.746,102.641 48.185,102.387 48.612,102.501C49.038,102.616 49.292," +
                    "103.055 49.177,103.481C49.063,103.908 48.624,104.161 48.198,104.047ZM40.728,101.328C40.328,101.142 40.155,100.665 40.341," +
                    "100.265C40.528,99.865 41.004,99.692 41.405,99.878C41.805,100.065 41.978,100.541 41.791,100.941C41.605,101.341 41.128," +
                    "101.515 40.728,101.328ZM27.755,92.245C27.443,91.932 27.443,91.425 27.755,91.113C28.068,90.801 28.575,90.801 28.887," +
                    "91.113C29.199,91.425 29.199,91.932 28.887,92.245C28.575,92.557 28.068,92.557 27.755,92.245ZM92.245,92.245C91.932," +
                    "92.557 91.425,92.557 91.113,92.245C90.801,91.932 90.801,91.425 91.113,91.113C91.425,90.801 91.932,90.801 92.245," +
                    "91.113C92.557,91.425 92.557,91.932 92.245,92.245ZM99.145,82.6C99.035,82.792 98.79,82.857 98.599,82.747C98.407," +
                    "82.636 98.342,82.392 98.452,82.2C98.562,82.009 98.807,81.944 98.999,82.054C99.19,82.164 99.255,82.409 99.145,82.6ZM20.855," +
                    "82.6C20.745,82.409 20.81,82.164 21.001,82.054C21.193,81.944 21.438,82.009 21.548,82.2C21.658,82.392 21.593,82.636 21.401," +
                    "82.747C21.21,82.857 20.965,82.792 20.855,82.6ZM104.047,71.802C103.933,72.229 103.494,72.482 103.067,72.368C102.641," +
                    "72.254 102.387,71.815 102.501,71.388C102.616,70.962 103.055,70.708 103.481,70.823C103.908,70.937 104.161,71.376 104.047," +
                    "71.802ZM15.953,71.802C15.839,71.376 16.092,70.937 16.519,70.823C16.945,70.708 17.384,70.962 17.499,71.388C17.613," +
                    "71.815 17.359,72.254 16.933,72.368C16.506,72.482 16.067,72.229 15.953,71.802ZM60,59.8C60.11,59.8 60.2,59.89 60.2,60C60.2," +
                    "60.11 60.11,60.2 60,60.2C59.89,60.2 59.8,60.11 59.8,60C59.8,59.89 59.89,59.8 60,59.8ZM104.047,48.198C104.161," +
                    "48.624 103.908,49.063 103.481,49.177C103.055,49.292 102.616,49.038 102.501,48.612C102.387,48.185 102.641,47.746 103.067," +
                    "47.632C103.494,47.518 103.933,47.771 104.047,48.198ZM15.953,48.198C16.067,47.771 16.506,47.518 16.933,47.632C17.359," +
                    "47.746 17.613,48.185 17.499,48.612C17.384,49.038 16.945,49.292 16.519,49.177C16.092,49.063 15.839,48.624 15.953," +
                    "48.198ZM27.755,27.755C28.068,27.443 28.575,27.443 28.887,27.755C29.199,28.068 29.199,28.575 28.887,28.887C28.575," +
                    "29.199 28.068,29.199 27.755,28.887C27.443,28.575 27.443,28.068 27.755,27.755ZM92.245,27.755C92.557,28.068 92.557," +
                    "28.575 92.245,28.887C91.932,29.199 91.425,29.199 91.113,28.887C90.801,28.575 90.801,28.068 91.113,27.755C91.425," +
                    "27.443 91.932,27.443 92.245,27.755ZM79.272,18.672C79.672,18.858 79.845,19.335 79.659,19.735C79.472,20.135 78.996," +
                    "20.308 78.595,20.122C78.195,19.935 78.022,19.459 78.209,19.059C78.395,18.659 78.872,18.485 79.272,18.672ZM48.198," +
                    "15.953C48.624,15.839 49.063,16.092 49.177,16.519C49.292,16.945 49.038,17.384 48.612,17.499C48.185,17.613 47.746," +
                    "17.359 47.632,16.933C47.518,16.506 47.771,16.067 48.198,15.953ZM71.802,15.953C72.229,16.067 72.482,16.506 72.368," +
                    "16.933C72.254,17.359 71.815,17.613 71.388,17.499C70.962,17.384 70.708,16.945 70.823,16.519C70.937,16.092 71.376," +
                    "15.839 71.802,15.953ZM60,14.799C60.221,14.799 60.4,14.979 60.4,15.199C60.4,15.42 60.221,15.599 60,15.599C59.779,15.599 59.6," +
                    "15.42 59.6,15.199C59.6,14.979 59.779,14.799 60,14.799Z' " +
                    $"{plateFillStyle}/>" +
                "</svg >";

            // Creates a fill for the 1/8 inch hole in the center of the plate, which will be added to the Stringbuilder
            string svgPlateEighthInchCenterHole =
                "<svg viewBox='0 0 120 120' style='fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;'>" +
                    "<g id='_1-8-inch-center-hole' serif:id='1/8 inch center hole' transform='matrix(0.0391862,0,0,0.0391862,57.5523,57.6538)'>" +
                        $"<circle cx='62.463' cy='59.874' r='5.104' {plateEighthInchCenterHoleStyle}/>" +
                    "</g>" +
                "</svg>";


            // Creates a fill for the three evenly space (120 Degrees apart) 1/4 inch holes in the plate, which will be added to the Stringbuilder
            string svgPlateQuarterInchHoles =
                "<svg viewBox='0 0 120 120' style='fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;'>" +
                    "<g id='_1-4-inch-holes'" +
                        "<g transform='matrix(-0.430075,-0.744911,0.744911,-0.430075,36.5204,131.127)'>" +
                            $"<circle cx='57.964' cy='12.901' r='0.465' {plateQuarterInchHoleStyle}/> " +
                        "</g>" +
                        "<g transform='matrix(-0.430075,0.744911,-0.744911,-0.430075,133.337,44.7706)'>" +
                            $"<circle cx='57.964' cy='12.901' r='0.465' {plateQuarterInchHoleStyle}/>" +
                        "</g>" +
                        "<g transform='matrix(0.860149,0,0,0.860149,10.1423,4.10277)'>" +
                            $"<circle cx='57.964' cy='12.901' r='0.465' {plateQuarterInchHoleStyle}/>" +
                        "</g>" +
                    "</g>" +
                "</svg>";

            // Creates a fill for the two evently spaced (180 Degrees apart) 1/2 inch holes in the plate, which will be added to the Stringbuilder
            string svgPlateHalfInch180DegreeSpacedHoles =
                "<svg viewBox='0 0 120 120' style='fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;'>" +
                    "<g id='_1-2-inch-holes--180-degree-spacing-'>" +
                        "<g transform='matrix(0.579984,0.270451,-0.270451,0.579984,48.2348,-5.80127)'>" +
                            $"<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch180DegreeSpacedHoleStyle}/>" +
                        "</g>" +
                        "<g transform='matrix(-0.579984,-0.270451,0.270451,-0.579984,71.7652,125.801)'>" +
                            $"<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch180DegreeSpacedHoleStyle}/>" +
                        "</g>" +
                    "</g>" +
                "</svg>";

            // Creates a fill for the twelve evently spaced (30 Degrees apart) 1/2 inch holes in the plate, which will be added to the Stringbuilder
            string svgPlateHalfInch30DegreeSpacedHoles =
                "<svg viewBox='0 0 120 120' style='fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;'>" +
                    "<g id='_1-2-inch-holes--30-degree-spacing--'>" +
                        "<g transform='matrix(0.452507,-0.452507,0.452507,0.452507,-5.8569,48.5503)'>" +
                            $"<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>" +
                        "</g>" +
                        "<g transform='matrix(0.165629,-0.618136,0.618136,0.165629,-2.75859,83.0127)'>" +
                            $"<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>" +
                        "</g>" +
                        "<g transform='matrix(-0.165629,-0.618136,0.618136,-0.165629,17.1558,111.309)'>" +
                            $"<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>" +
                        "</g>" +
                        "<g transform='matrix(-0.452507,-0.452507,0.452507,-0.452507,48.5503,125.857)'>" +
                            $"<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>" +
                        "</g>" +
                        "<g transform='matrix(-0.618136,-0.165629,0.165629,-0.618136,83.0127,122.759)'>" +
                            $"<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>" +
                        "</g>" +
                        "<g transform='matrix(-0.618136,0.165629,-0.165629,-0.618136,111.309,102.844)'>" +
                            $"<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>" +
                        "</g>" +
                        "<g transform='matrix(-0.452507,0.452507,-0.452507,-0.452507,125.857,71.4497)'>" +
                            $"<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>" +
                        "</g>" +
                        "<g transform='matrix(-0.165629,0.618136,-0.618136,-0.165629,122.759,36.9873)'>" +
                            $"<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>" +
                        "</g>" +
                        "<g transform='matrix(0.165629,0.618136,-0.618136,0.165629,102.844,8.6911)'>" +
                            $"<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>" +
                        "</g>" +
                        "<g transform='matrix(0.452507,0.452507,-0.452507,0.452507,71.4497,-5.8569)'>" +
                            $"<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>" +
                        "</g>" +
                        "<g transform='matrix(0.618136,0.165629,-0.165629,0.618136,36.9873,-2.75859)'>" +
                            $"<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>" +
                        "</g>" +
                        "<g transform='matrix(0.618136,-0.165629,0.165629,0.618136,8.6911,17.1558)'>" +
                            $"<circle cx='60.118' cy='15.413' r='1.25' {plateHalfInch30DegreeSpacedHoleStyle}/>" +
                        "</g>" +
                    "</g>" +
                "</svg>";

            // Creates a new StringBuilder object
            var svgStringBuilder = new StringBuilder();

            // Adds the some of the SVG strings to the Stringbuilder
            svgStringBuilder.Append(svgOpenOuter);
            svgStringBuilder.Append(svgPlateBorder);
            svgStringBuilder.Append(svgPlateFill);
            svgStringBuilder.Append(svgPlateEighthInchCenterHole);
            svgStringBuilder.Append(svgPlateQuarterInchHoles);
            svgStringBuilder.Append(svgPlateHalfInch180DegreeSpacedHoles);
            svgStringBuilder.Append(svgPlateHalfInch30DegreeSpacedHoles);


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

                switch (stellarObjectData[i][6])
                {
                    case "GALAXY":
                        dotStrokeColor = galaxyStrokeColor;
                        dotFillColor = galaxyFillColor;
                        break;
                    case "STAR":
                        dotStrokeColor = starStrokeColor;
                        dotFillColor = starFillColor;
                        break;
                    case "QSO":
                        dotStrokeColor = qsoStrokeColor;
                        dotFillColor = qsoFillColor;
                        break;
                    default:
                        break;
                }

                svgStringBuilder.Append(
                    // Ampersands "&" in the href query string have been replaced with "&amp;" since a regular Ampersand is a escapement character in XML (svg).
                    $"<a href='https://skyserver.sdss.org/dr17/VisualTools/navi?ra={stellarObjectData[i][4]}&amp;dec={stellarObjectData[i][5]}&amp;scale={photoScaler}' target='_blank'> " +
                    $"<circle cx='{cxScaledAndTranslatedString}' cy='{cyScaledAndTranslatedString}' r='{dotAreaScaler}' stroke='{dotStrokeColor}' stroke-width='{strokeWidth}' fill='{dotFillColor}'/>" +
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

            ////////////////////////////////
            ///// Adjustable Constants /////
            ////////////////////////////////
            
            // Creates the Background Color of the website behind the plate svg
            string backgroundColor = "style='background-color:rgb(0,0,0);'";

            // Creates a limit on how far panzoom will zoom into the plate svg
            string maxScale = "maxScale: 50";

            // Creates a limit on how far panzoom will zoom out of the plate svg
            string minScale = "minScale: .75";

            ////////////////////////////////
            ////////////////////////////////

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
                $"<body {backgroundColor}>" +
                    "<div id='svgWrapper' style='display: flex; justify-content: center; align-items: center'>" +
                 "";

            string htmlClose = "" +
                    "</div>" +
                    "<script> " +
                        "const element = document.getElementById('svgWrapper')," +
                        // the below reads like it has a "const" at the beginning because of the comma from the above line.
                        "panzoom = Panzoom(element, {" +
                        // options here
                        $"{maxScale}," +
                        $"{minScale}" +
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